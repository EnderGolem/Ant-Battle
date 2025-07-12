import pygame
import math
import json
import websocket
import threading
import time
import signal
import sys
from collections import defaultdict

discovered_hexes = {}  # ключ (q, r): hex_type
discovered_food = {}  # ключ: (q, r) -> словарь с type и amount

pygame.mixer.init()
start_round_sound = pygame.mixer.Sound("begin.wav")
end_round_sound = pygame.mixer.Sound("end.wav")


# Инициализация Pygame
pygame.init()

# Настройки экрана
WIDTH, HEIGHT = 1600,900
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Ant Arena Visualizer")

# Загрузка текстур еды
try:
    texture_apple = pygame.image.load("apple.png").convert_alpha()
    texture_bread = pygame.image.load("bread.png").convert_alpha()
    texture_nectar = pygame.image.load("nectar.png").convert_alpha()
except Exception as e:
    print(f"[Error] Не удалось загрузить текстуры еды: {e}")
    texture_apple = texture_bread = texture_nectar = None

# Настройки сервера
WS_URL = "ws://37.48.249.190:8080/echo"
PING_INTERVAL = 10  # seconds

# Цвета
BACKGROUND = (0, 0, 0)
HEX_LINE_COLOR = (0, 0, 0)
TEXT_COLOR = (255, 255, 255)
TEXT_ALLY = (173, 216, 230)   # Светло-голубой (Light Blue)
TEXT__ENEMY = (255, 182, 193) # Светло-красный (Light Red / Light Pink)
TEXT_FOOD = (255, 255, 153)   # Светло-жёлтый (Light Yellow)
COORD_TEXT_COLOR = (255, 255, 0)  # Желтый цвет для координат

# Параметры шестиугольника
BASE_HEX_SIZE = 20
BASE_HEX_WIDTH = BASE_HEX_SIZE * math.sqrt(3)
BASE_HEX_HEIGHT = BASE_HEX_SIZE * 2
BASE_HEX_VERTICAL_SPACING = BASE_HEX_SIZE * 1.5

# Камера
camera_x, camera_y = 0, 0
zoom_level = 1.0
MIN_ZOOM = 0.1
MAX_ZOOM = 5.0
dragging = False
last_mouse_pos = (0, 0)

# Шрифты
font = pygame.font.SysFont('Arial', 16)
small_font = pygame.font.SysFont('Arial', 12)

# Типы объектов
ANT_TYPE_SCOUT = 2
ANT_TYPE_FIGHTER = 1
ANT_TYPE_WORKER = 0

HEX_HOME = 1
HEX_EMPTY = 2
HEX_DIRT = 3
HEX_ACID = 4
HEX_ROCKS = 5

FOOD_APPLE = 1
FOOD_BREAD = 2
FOOD_NECTAR = 3

# Цвета

HEX_COLORS = {
    HEX_HOME: (255, 105, 180),
    HEX_EMPTY: (34, 139, 34),
    HEX_DIRT: (101, 67, 33),
    HEX_ACID: (0, 100, 0),
    HEX_ROCKS: (105, 105, 105)
}

FOOD_COLORS = {
    FOOD_APPLE: (255, 0, 0),
    FOOD_BREAD: (210, 180, 140),
    FOOD_NECTAR: (255, 215, 0)
}

# Глобальные переменные для WebSocket
ws = None
ping_thread = None
ws_thread = None
running = True
game_state = None
connection_failed = False
last_reconnect_time = 0
RECONNECT_COOLDOWN = 2  # seconds

# Режим отображения координат (True - axial, False - odd-r)
axial_view = False

# WebSocket функции
def on_open(ws):
    global connection_failed
    print("[WebSocket] Connected")
    connection_failed = False
    ws.send("subscribe")

def on_message(ws, message):
    global game_state
    try:
        oldTurn = game_state.get('turnNo', 0)
        first_state = (game_state == None)
        game_state = json.loads(message)
        turn = game_state.get('turnNo', 0)
        if (turn == 0 and oldTurn != 0):
            end_round_sound.play()
            discovered_hexes.clear()
            discovered_food.clear()
        if (turn == 1 and oldTurn != 1): #Начало раунда
            start_round_sound.play()
            center_on_home()
        if (turn > 0 and first_state):
            center_on_home()
    except json.JSONDecodeError as e:
        print(f"[WebSocket] Error decoding message: {e}")

def on_pong(ws, frame_data):
    print("[WebSocket] Pong received")

def on_close(ws, close_status_code, close_msg):
    global connection_failed
    print(f"[WebSocket] Disconnected: {close_status_code} {close_msg}")
    connection_failed = True

def on_error(ws, error):
    global connection_failed
    print(f"[WebSocket] Error: {error}")
    connection_failed = True

def send_pings():
    while running:
        try:
            if ws and hasattr(ws, 'sock') and ws.sock and ws.sock.connected:
                print("[WebSocket] Sending Ping")
                ws.sock.ping()
            else:
                print("[WebSocket] Skip ping - not connected")
        except Exception as e:
            print(f"[WebSocket] Ping failed: {e}")
        time.sleep(PING_INTERVAL)

def graceful_exit(signum, frame):
    global running
    print("\n[WebSocket] Exiting...")
    running = False
    if ws:
        try:
            ws.send("unsubscribe")
            time.sleep(0.5)
            ws.close()
        except:
            pass
    pygame.quit()
    sys.exit(0)

def cleanup_websocket():
    global ws, ping_thread, ws_thread
    if ws:
        try:
            ws.close()
        except:
            pass
    if ping_thread and ping_thread.is_alive():
        ping_thread.join(timeout=0.5)
    if ws_thread and ws_thread.is_alive():
        ws_thread.join(timeout=0.5)

def connect_websocket():
    global ws, ping_thread, ws_thread, last_reconnect_time
    
    current_time = time.time()
    if current_time - last_reconnect_time < RECONNECT_COOLDOWN:
        print(f"[WebSocket] Reconnect cooldown - wait {RECONNECT_COOLDOWN} seconds")
        return ws
    
    last_reconnect_time = current_time
    
    print("[WebSocket] Connecting...")
    
    # Cleanup previous connection
    cleanup_websocket()
    
    ws = websocket.WebSocketApp(
        WS_URL,
        on_open=on_open,
        on_message=on_message,
        on_pong=on_pong,
        on_close=on_close,
        on_error=on_error
    )

    # Запуск ping в отдельном потоке
    ping_thread = threading.Thread(target=send_pings)
    ping_thread.daemon = True
    ping_thread.start()

    # Запуск клиента в отдельном потоке
    ws_thread = threading.Thread(target=ws.run_forever, kwargs={'ping_interval': None})
    ws_thread.daemon = True
    ws_thread.start()

    return ws

# Визуализация
def hex_corner(center, size, i):
    angle_deg = 60 * i - 30
    angle_rad = math.pi / 180 * angle_deg
    return (center[0] + size * math.cos(angle_rad),
            center[1] + size * math.sin(angle_rad))

def draw_hexagon(center, size, color, line_color):
    points = [hex_corner(center, size, i) for i in range(6)]
    pygame.draw.polygon(screen, color, points)
    pygame.draw.polygon(screen, line_color, points, 2)

def screen_to_hex(pos):
    x, y = pos
    world_x = (x - WIDTH/2) / zoom_level + camera_x
    world_y = (y - HEIGHT/2) / zoom_level + camera_y
    
    if axial_view:
        # Преобразование для axial координат
        q = (math.sqrt(3)/3 * world_x - 1./3 * world_y) / BASE_HEX_SIZE
        r = (2./3 * world_y) / BASE_HEX_SIZE
        axial_q, axial_r = axial_round(q, r)
        return (axial_q, axial_r)
    else:
        # Преобразование для odd-r координат
        row = round(world_y / BASE_HEX_VERTICAL_SPACING)
        col = round((world_x - (row % 2) * BASE_HEX_WIDTH / 2) / BASE_HEX_WIDTH)
        return (int(col), int(row))

def axial_round(q, r):
    """Округление осевых координат до ближайшего гекса"""
    x = q
    z = r
    y = -x - z
    
    rx = round(x)
    ry = round(y)
    rz = round(z)
    
    x_diff = abs(rx - x)
    y_diff = abs(ry - y)
    z_diff = abs(rz - z)
    
    if x_diff > y_diff and x_diff > z_diff:
        rx = -ry - rz
    elif y_diff > z_diff:
        ry = -rx - rz
    else:
        rz = -rx - ry
    
    return (rx, rz)

def get_hex_position(q, r):
    """Возвращает экранные координаты для гекса с координатами (q, r) в текущем режиме отображения"""
    if axial_view:
        # Для axial координат
        x = BASE_HEX_SIZE * (math.sqrt(3) * q + math.sqrt(3)/2 * r)
        y = BASE_HEX_SIZE * (3./2 * r)
    else:
        # Для odd-r координат
        x = q * BASE_HEX_WIDTH
        if r % 2 == 1:
            x += BASE_HEX_WIDTH / 2
        y = r * BASE_HEX_VERTICAL_SPACING
    
    screen_x = WIDTH/2 + (x - camera_x) * zoom_level
    screen_y = HEIGHT/2 + (y - camera_y) * zoom_level
    return (screen_x, screen_y)

def draw_ant(center, size, ant_type, is_enemy, food=None):
    fill_color = (255, 0, 0) if is_enemy else (64, 64, 255)  # Цвет тела
    outline_color = (0, 0, 0)  # Чёрная окантовка

    # Окантовка — рисуется чуть больше основного размера
    outline_scale = 1.2
    
    if ant_type == ANT_TYPE_SCOUT:
        # Контур
        outline_points = [
            (center[0], center[1] - size * outline_scale),
            (center[0] - size * 0.8 * outline_scale, center[1] + size * 0.6 * outline_scale),
            (center[0] + size * 0.8 * outline_scale, center[1] + size * 0.6 * outline_scale)
        ]
        pygame.draw.polygon(screen, outline_color, outline_points)
        # Тело
        points = [
            (center[0], center[1] - size),
            (center[0] - size * 0.8, center[1] + size * 0.6),
            (center[0] + size * 0.8, center[1] + size * 0.6)
        ]
        pygame.draw.polygon(screen, fill_color, points)

    elif ant_type == ANT_TYPE_FIGHTER:
        draw_hexagon(center, size * 0.7 * outline_scale, outline_color, outline_color)
        draw_hexagon(center, size * 0.7, fill_color, fill_color)

    else:  # worker
        pygame.draw.circle(screen, outline_color, center, int(size * 0.8 * outline_scale))
        pygame.draw.circle(screen, fill_color, center, int(size * 0.8))

    # Рисуем еду, если муравей её несёт
    if food and food.get('amount', 0) > 0:
        food_size = size
        food_pos = (center[0], center[1])  # Над муравьём
        
        # Создаём поверхность для еды
        food_surface = pygame.Surface((food_size * 2, food_size * 2), pygame.SRCALPHA)
        draw_food(food_surface, (food_size, food_size), food_size, food['type'], food['amount'])
        screen.blit(food_surface, (food_pos[0] - food_size, food_pos[1] - food_size))

def draw_food(surface, center, size, food_type, amount):
    image = None
    if food_type == FOOD_APPLE:
        image = texture_apple
    elif food_type == FOOD_BREAD:
        image = texture_bread
    elif food_type == FOOD_NECTAR:
        image = texture_nectar

    if image:
        # Масштабируем изображение под размер гекса
        scaled_size = int(size * 1.1)
        image = pygame.transform.smoothscale(image, (scaled_size, scaled_size))
        rect = image.get_rect(center=center)
        surface.blit(image, rect)
    else:
        # Фоллбэк — цветная фигура
        color = FOOD_COLORS.get(food_type, (255, 255, 255))
        pygame.draw.circle(surface, color, center, int(size * 0.6))

    # Отображаем количество
    if zoom_level > 0.3:
        amount_text = font.render(str(amount), True, TEXT_FOOD)
        surface.blit(amount_text, (center[0] - 5, center[1]))

def draw_star(center, size, color):
    points = []
    for i in range(5):
        angle = math.pi / 2 - 2 * math.pi / 5 * i
        points.append((center[0] + math.cos(angle) * size, 
                      center[1] - math.sin(angle) * size))
        inner_angle = angle + math.pi / 5
        points.append((center[0] + math.cos(inner_angle) * size * 0.4, 
                      center[1] - math.sin(inner_angle) * size * 0.4))
    pygame.draw.polygon(screen, color, points)

def draw_arrow(surface, start, end, color=(255, 255, 255), width=2, head_size=8, shrink=4):
    """
    Рисует стрелку от start к end, с усечением на длину `shrink` с обоих сторон.
    """
    dx = end[0] - start[0]
    dy = end[1] - start[1]
    distance = math.hypot(dx, dy)

    if distance == 0:
        return  # ничего не рисуем, если длина 0

    # Направление вектора
    unit_dx = dx / distance
    unit_dy = dy / distance

    # Сдвигаем начало и конец внутрь отрезка
    new_start = (
        start[0] + unit_dx * shrink,
        start[1] + unit_dy * shrink
    )
    new_end = (
        end[0] - unit_dx * shrink,
        end[1] - unit_dy * shrink
    )

    # Основная линия стрелки
    pygame.draw.line(surface, color, new_start, new_end, width)

    # Наконечник (треугольник)
    angle = math.atan2(new_end[1] - new_start[1], new_end[0] - new_start[0])
    x1 = new_end[0] - head_size * math.cos(angle - math.pi / 6)
    y1 = new_end[1] - head_size * math.sin(angle - math.pi / 6)
    x2 = new_end[0] - head_size * math.cos(angle + math.pi / 6)
    y2 = new_end[1] - head_size * math.sin(angle + math.pi / 6)

    pygame.draw.polygon(surface, color, [new_end, (x1, y1), (x2, y2)])

def draw_game_state():
    if not game_state:
        return {}

    food_count = 0
    my_ants = 0
    enemy_ants = 0
    visible_cells = len(game_state.get('map', []))
    total_points = game_state.get('score', 0)
    turn_number = game_state.get('turnNo', 0)

    # Рисуем гексы
    for hex_data in game_state.get('map', []):
        q, r = hex_data['q'], hex_data['r']
        hex_type = hex_data['type']
        discovered_hexes[(q, r)] = hex_type
        
        screen_x, screen_y = get_hex_position(q, r)
        
        draw_hexagon((screen_x, screen_y), BASE_HEX_SIZE * zoom_level, 
                    HEX_COLORS.get(hex_type, (50, 50, 150)), HEX_LINE_COLOR)
        
    visible_coords = {(cell['q'], cell['r']) for cell in game_state.get('map', [])}

    current_visible_food = {}
    for food in game_state.get('food', []):
        coord = (food['q'], food['r'])
        current_visible_food[coord] = food
        discovered_food[coord] = food  # сохраняем/обновляем

    # Удаляем еду из тех клеток, которые видимы, но еды больше нет
    for coord in list(discovered_food.keys()):
        if coord in visible_coords and coord not in current_visible_food:
            del discovered_food[coord]

    for (q, r), hex_type in discovered_hexes.items():
        if (q, r) in visible_coords:
            continue
        
        screen_x, screen_y = get_hex_position(q, r)

        # Создание полупрозрачной поверхности
        size = BASE_HEX_SIZE * zoom_level
        surface = pygame.Surface((size * 2, size * 2), pygame.SRCALPHA)
        color = HEX_COLORS.get(hex_type, (50, 50, 150))
        translucent_color = (*color, 80)  # Прозрачность 80/255

        center = (size, size)
        points = [(
            center[0] + size * math.cos(math.pi / 180 * (60 * i - 30)),
            center[1] + size * math.sin(math.pi / 180 * (60 * i - 30))
        ) for i in range(6)]

        pygame.draw.polygon(surface, translucent_color, points)
        screen.blit(surface, (screen_x - size, screen_y - size))

    # Рисуем муравейник
    for home in game_state.get('home', []):
        q, r = home['q'], home['r']
        screen_x, screen_y = get_hex_position(q, r)
        
        pygame.draw.circle(screen, (255, 215, 0), (int(screen_x), int(screen_y)), int(5 * zoom_level))

    # Рисуем ресурсы
    for (q, r), food in discovered_food.items():
        food_count += 1

        screen_x, screen_y = get_hex_position(q, r)

        # Полупрозрачность для скрытой еды
        visible = (q, r) in visible_coords
        if visible:
            draw_food(screen, (screen_x, screen_y), BASE_HEX_SIZE * zoom_level, food['type'], food['amount'])
        else:
            # Рисуем на полупрозрачной поверхности
            surface = pygame.Surface((BASE_HEX_SIZE * 2 * zoom_level, BASE_HEX_SIZE * 2 * zoom_level), pygame.SRCALPHA)
            center = (BASE_HEX_SIZE * zoom_level, BASE_HEX_SIZE * zoom_level)
            draw_food(surface, center, BASE_HEX_SIZE * zoom_level, food['type'], food['amount'])
            surface.set_alpha(80)
            screen.blit(surface, (screen_x - center[0], screen_y - center[1]))

    # Собираем всех муравьев (союзных и вражеских) для отображения
    all_ants = defaultdict(list)
    
    # Союзные муравьи
    for ant in game_state.get('ants', []):
        all_ants[(ant['q'], ant['r'])].append((ant, False))  # False - не враг
        my_ants += 1
    
    # Вражеские муравьи
    for enemy in game_state.get('enemies', []):
        all_ants[(enemy['q'], enemy['r'])].append((enemy, True))  # True - враг
        enemy_ants += 1

    # Функция для отрисовки группы муравьев в гексе
    def draw_ants_in_hex(q, r, ants_data):
        screen_x, screen_y = get_hex_position(q, r)
        num_ants = len(ants_data)
        radius = BASE_HEX_SIZE * zoom_level * 0.5

        # Если муравей один - позиционируем по центру гекса
        if num_ants == 1:
            positions = [(0, 0)]  # Центральная позиция
        else:
            # Позиции для 2-3 юнитов (в равностороннем треугольнике)
            positions = [
                (0, -radius),
                (-radius * math.cos(math.pi / 6), radius * math.sin(math.pi / 6)),
                (radius * math.cos(math.pi / 6), radius * math.sin(math.pi / 6))
            ]

        for i, (ant, is_enemy) in enumerate(ants_data):
            if i >= 3:
                break  # максимум 3 юнита на гекс
            offset_x, offset_y = positions[i]
            cx = screen_x + offset_x
            cy = screen_y + offset_y

            last_moves = ant.get('lastMove')
            if isinstance(last_moves, list) and len(last_moves) >= 2:
                for i in range(len(last_moves) - 1):
                    start_qr = last_moves[i]
                    end_qr = last_moves[i + 1]

                    if not all(k in start_qr and k in end_qr for k in ('q', 'r')):
                        continue  # пропуск некорректных данных

                    # Преобразование в экранные координаты
                    def hex_to_screen(q, r):
                        if axial_view:
                            x = BASE_HEX_SIZE * (math.sqrt(3) * q + math.sqrt(3)/2 * r)
                            y = BASE_HEX_SIZE * (3./2 * r)
                        else:
                            x = q * BASE_HEX_WIDTH
                            if int(r) % 2 == 1:
                                x += BASE_HEX_WIDTH / 2
                            y = r * BASE_HEX_VERTICAL_SPACING
                        sx = WIDTH / 2 + (x - camera_x) * zoom_level
                        sy = HEIGHT / 2 + (y - camera_y) * zoom_level
                        return (sx, sy)

                    start_pos = hex_to_screen(float(start_qr["q"]), float(start_qr["r"]))
                    end_pos = hex_to_screen(float(end_qr["q"]), float(end_qr["r"]))

                    # Нарисовать стрелку
                    draw_arrow(screen, start_pos, end_pos, color=(200, 200, 255), width=2)

            ant_food = ant.get('food')
            draw_ant((cx, cy), BASE_HEX_SIZE * zoom_level * 0.4, ant['type'], is_enemy, ant_food)

            # ХП над каждым
            if zoom_level > 0.5:
                if is_enemy:
                    health_text = small_font.render(str(ant['health']), True, TEXT__ENEMY)
                else:
                    health_text = small_font.render(str(ant['health']), True, TEXT_ALLY)

                screen.blit(health_text, (cx - 10, cy - 20))

    # Отрисовываем всех муравьев
    for (q, r), ants_data in all_ants.items():
        draw_ants_in_hex(q, r, ants_data)

    return {
        "turn": turn_number,
        "points": total_points,
        "my_ants": my_ants,
        "enemies": enemy_ants,
        "food": food_count,
        "cells": visible_cells
    }

def load_game_state_from_file():
    global game_state, camera_x, camera_y
    try:
        with open('sample.json', 'r') as f:
            game_state = json.load(f)
            print("Состояние игры успешно загружено из файла sample.json")
            
            # Автоматически центрируем на доме после загрузки
            center_on_home()
    except Exception as e:
        print(f"Ошибка при загрузке состояния игры: {e}")

def center_on_home():
    global camera_x, camera_y, zoom_level
    if not game_state or not game_state.get('home'):
        return
    
    # Берем первый муравейник (можно добавить логику для выбора своего муравейника)
    home = game_state['home'][0]
    q, r = home['q'], home['r']
    
    screen_x, screen_y = get_hex_position(q, r)
    
    # Преобразуем обратно в мировые координаты
    camera_x = (screen_x - WIDTH/2) / zoom_level + camera_x
    camera_y = (screen_y - HEIGHT/2) / zoom_level + camera_y
    zoom_level = 1.0  # Сбрасываем зум при центрировании

def main():
    global camera_x, camera_y, zoom_level, dragging, last_mouse_pos, ws, running, connection_failed, axial_view

    signal.signal(signal.SIGINT, graceful_exit)
    connect_websocket()

    clock = pygame.time.Clock()

    r_was_pressed = False

    while running:
        screen.fill(BACKGROUND)

        # Обработка событий
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            elif event.type == pygame.MOUSEWHEEL:
                mouse_x, mouse_y = pygame.mouse.get_pos()
                world_x_before = (mouse_x - WIDTH/2) / zoom_level + camera_x
                world_y_before = (mouse_y - HEIGHT/2) / zoom_level + camera_y

                if event.y > 0:
                    zoom_level = min(zoom_level * 1.1, MAX_ZOOM)
                elif event.y < 0:
                    zoom_level = max(zoom_level * 0.9, MIN_ZOOM)

                camera_x = world_x_before - (mouse_x - WIDTH/2) / zoom_level
                camera_y = world_y_before - (mouse_y - HEIGHT/2) / zoom_level

            elif event.type == pygame.MOUSEBUTTONDOWN:
                if event.button == 2:
                    dragging = True
                    last_mouse_pos = event.pos
            elif event.type == pygame.MOUSEBUTTONUP:
                if event.button == 2:
                    dragging = False
            elif event.type == pygame.MOUSEMOTION and dragging:
                dx = (event.pos[0] - last_mouse_pos[0]) / zoom_level
                dy = (event.pos[1] - last_mouse_pos[1]) / zoom_level
                camera_x -= dx
                camera_y -= dy
                last_mouse_pos = event.pos
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_s:
                    load_game_state_from_file()
                elif event.key == pygame.K_SPACE:
                    center_on_home()
                elif event.key == pygame.K_n:
                    discovered_hexes.clear()
                    discovered_food.clear()
                    print("Обнаруженные клетки сброшены.")
                elif event.key == pygame.K_a:
                    axial_view = not axial_view
                    print(f"Переключено в режим {'axial' if axial_view else 'odd-r'} координат")

        # Клавиши
        keys = pygame.key.get_pressed()
        if keys[pygame.K_LEFT]:
            camera_x -= 10 / zoom_level
        if keys[pygame.K_RIGHT]:
            camera_x += 10 / zoom_level
        if keys[pygame.K_UP]:
            camera_y -= 10 / zoom_level
        if keys[pygame.K_DOWN]:
            camera_y += 10 / zoom_level
        if keys[pygame.K_r]:
            if not r_was_pressed:
                if ws:
                    ws.close()
                connect_websocket()
                print("Попытка переподключения...")
                connection_failed = False
                r_was_pressed = True
        else:
            r_was_pressed = False

        # Отрисовка состояния игры
        if game_state:
            stats = draw_game_state()
            info_lines = [
                f"Ход: {stats['turn']}",
                f"Очки: {stats['points']}",
                f"Юниты: {stats['my_ants']}",
                f"Противники: {stats['enemies']}",
                f"Еда: {stats['food']}",
                f"Видимые клетки: {stats['cells']}",
                f"Изученные клетки: {len(discovered_hexes)}",
                f"Режим: {'axial' if axial_view else 'odd-r'} ('A' - переключить)"
            ]
            for i, line in enumerate(info_lines):
                info_text = font.render(line, True, TEXT_COLOR)
                screen.blit(info_text, (10, 80 + i * 20))
        else:
            no_state_text = font.render("Нет данных о состоянии игры ('S' - загрузить данные из sample.json)", True, (255, 255, 0))
            screen.blit(no_state_text, (10, 40))

        # Отображение координат
        mouse_pos = pygame.mouse.get_pos()
        hex_coords = screen_to_hex(mouse_pos)
        coord_text = font.render(f"Hex: {hex_coords[0]}, {hex_coords[1]}", True, COORD_TEXT_COLOR)
        screen.blit(coord_text, (mouse_pos[0] + 10, mouse_pos[1] + 10))

        # Отрисовка обводки гекса под курсором
        q, r = hex_coords
        screen_x, screen_y = get_hex_position(q, r)

        # Рисуем желтую обводку
        points = [hex_corner((screen_x, screen_y), BASE_HEX_SIZE * zoom_level, i) for i in range(6)]
        pygame.draw.polygon(screen, (255, 255, 0), points, 1)  # Толщина обводки 3 пикселя

        # Собираем информацию о гексе
        hex_info = []
        q, r = hex_coords

        # Информация о типе гекса
        if (q, r) in discovered_hexes:
            hex_type = discovered_hexes[(q, r)]
            type_names = {
                HEX_HOME: "Муравейник",
                HEX_EMPTY: "Пусто",
                HEX_DIRT: "Земля",
                HEX_ACID: "Кислота",
                HEX_ROCKS: "Камни"
            }
            hex_info.append(f"Тип: {type_names.get(hex_type, 'Неизвестно')}")

        # Информация о еде
        if (q, r) in discovered_food:
            food = discovered_food[(q, r)]
            food_names = {
                FOOD_APPLE: "Яблоко",
                FOOD_BREAD: "Хлеб",
                FOOD_NECTAR: "Нектар"
            }
            hex_info.append(f"Еда: {food_names.get(food['type'], 'Неизвестно')} ({food['amount']})")

        # Информация о юнитах
        if game_state:
            ants_on_hex = []
            
            # Союзные муравьи
            for ant in game_state.get('ants', []):
                if ant['q'] == q and ant['r'] == r:
                    ant_type_names = {
                        ANT_TYPE_WORKER: "Рабочий",
                        ANT_TYPE_FIGHTER: "Воин",
                        ANT_TYPE_SCOUT: "Разведчик"
                    }
                    ants_on_hex.append(f"Союзник: {ant_type_names.get(ant['type'], 'Неизвестно')} (HP: {ant['health']}) ID: {ant.get('id', '?')}")
            
            # Вражеские муравьи
            for enemy in game_state.get('enemies', []):
                if enemy['q'] == q and enemy['r'] == r:
                    ant_type_names = {
                        ANT_TYPE_WORKER: "Рабочий",
                        ANT_TYPE_FIGHTER: "Воин",
                        ANT_TYPE_SCOUT: "Разведчик"
                    }
                    ants_on_hex.append(f"Враг: {ant_type_names.get(enemy['type'], 'Неизвестно')} (HP: {enemy['health']}) ID: {enemy.get('id', '?')}")

            if ants_on_hex:
                hex_info.append("Юниты:")
                hex_info.extend(ants_on_hex)

        # Отображаем информацию о гексе
        if hex_info:
            title = font.render(f"Гекс ({q}, {r})", True, (255, 255, 0))
            title_height = title.get_height()
            line_height = title_height  # Все строки тем же шрифтом

            padding = 5
            info_height = padding + title_height + padding + len(hex_info) * line_height + padding
            info_surface = pygame.Surface((350, info_height), pygame.SRCALPHA)
            info_surface.fill((0, 0, 0, 180))  # Полупрозрачный фон

            # Заголовок
            info_surface.blit(title, (10, padding))

            # Основная информация — тем же шрифтом
            for i, line in enumerate(hex_info):
                text = font.render(line, True, (255, 255, 255))
                info_surface.blit(text, (10, padding + title_height + padding + i * line_height))

            # Позиционирование
            info_x = mouse_pos[0] + 20
            info_y = mouse_pos[1]

            if info_x + info_surface.get_width() > WIDTH:
                info_x = mouse_pos[0] - info_surface.get_width() - 20
            if info_y + info_surface.get_height() > HEIGHT:
                info_y = HEIGHT - info_surface.get_height()

            screen.blit(info_surface, (info_x, info_y))
        # Отображение параметров камеры
        camera_text = font.render(f"Камера: ({camera_x:.1f}, {camera_y:.1f}) Масштаб: {zoom_level:.2f}x", True, TEXT_COLOR)
        screen.blit(camera_text, (10, HEIGHT - 30))

        # Отрисовка состояния подключения
        if connection_failed:
            conn_status = "ОТКЛЮЧЕНО ('R' - повторить попытку)"
            conn_color = (255, 0, 0)
        elif ws and ws.sock and ws.sock.connected:
            conn_status = "ПОДКЛЮЧЕНО"
            conn_color = (0, 255, 0)
        else:
            conn_status = "ПОДКЛЮЧЕНИЕ..."
            conn_color = (255, 165, 0)

        conn_text = font.render(f"Сервер: {conn_status}", True, conn_color)
        screen.blit(conn_text, (10, 10))

        pygame.display.flip()
        clock.tick(60)

    graceful_exit(None, None)

if __name__ == "__main__":
    main()