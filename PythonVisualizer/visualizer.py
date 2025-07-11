import pygame
import math
import json
import websocket
import threading
import time
import signal
import sys
from collections import defaultdict

# Инициализация Pygame
pygame.init()

# Настройки экрана
WIDTH, HEIGHT = 1200, 800
screen = pygame.display.set_mode((WIDTH, HEIGHT))
pygame.display.set_caption("Ant Arena Visualizer")

# Настройки сервера
WS_URL = "ws://37.48.249.190:8080/echo"
PING_INTERVAL = 10  # seconds

# Глобальные переменные для WebSocket
ws = None
ping_thread = None
running = True
game_state = None
connection_failed = False

# Цвета
BACKGROUND = (0, 0, 0)
HEX_LINE_COLOR = (100, 100, 255)
TEXT_COLOR = (255, 255, 255)
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
ANT_COLORS = {
    ANT_TYPE_SCOUT: (0, 191, 255),
    ANT_TYPE_FIGHTER: (255, 69, 0),
    ANT_TYPE_WORKER: (50, 205, 50)
}

HEX_COLORS = {
    HEX_HOME: (139, 69, 19),
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

# WebSocket функции
def on_open(ws):
    print("[WebSocket] Connected")
    ws.send("subscribe")

def on_message(ws, message):
    global game_state
    try:
        game_state = json.loads(message)
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
        print("[WebSocket] Sending Ping")
        try:
            if ws.sock and ws.sock.connected:
                ws.sock.ping()
        except Exception as e:
            print(f"[WebSocket] Ping failed: {e}")
            break
        time.sleep(PING_INTERVAL)

def graceful_exit(signum, frame):
    global running
    print("\n[WebSocket] Exiting...")
    running = False
    if ws:
        ws.send("unsubscribe")
        time.sleep(0.5)
        ws.close()
    pygame.quit()
    sys.exit(0)

def connect_websocket():
    global ws, ping_thread
    print("[WebSocket] Connecting...")
    
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
    row = round(world_y / BASE_HEX_VERTICAL_SPACING)
    col = round((world_x - (row % 2) * BASE_HEX_WIDTH / 2) / BASE_HEX_WIDTH)
    return (int(col), int(row))

def draw_ant(center, size, ant_type):
    color = ANT_COLORS.get(ant_type, (255, 255, 255))
    
    if ant_type == ANT_TYPE_SCOUT:
        points = [
            (center[0], center[1] - size),
            (center[0] - size * 0.8, center[1] + size * 0.6),
            (center[0] + size * 0.8, center[1] + size * 0.6)
        ]
        pygame.draw.polygon(screen, color, points)
    elif ant_type == ANT_TYPE_FIGHTER:
        draw_hexagon(center, size * 0.7, color, color)
    else:
        pygame.draw.circle(screen, color, center, int(size * 0.8))

def draw_food(center, size, food_type, amount):
    color = FOOD_COLORS.get(food_type, (255, 255, 255))
    
    if food_type == FOOD_APPLE:
        pygame.draw.circle(screen, color, center, int(size * 0.6))
        pygame.draw.line(screen, (101, 67, 33), 
                        (center[0], center[1] - size * 0.6),
                        (center[0] - size * 0.3, center[1] - size * 0.8), 2)
    elif food_type == FOOD_BREAD:
        pygame.draw.rect(screen, color, 
                         (center[0] - size * 0.5, center[1] - size * 0.4, 
                          size, size * 0.8))
    else:
        draw_star(center, size * 0.7, color)
    
    # Отображаем количество
    amount_text = small_font.render(str(amount), True, TEXT_COLOR)
    screen.blit(amount_text, (center[0] - 5, center[1] + size * 0.7))

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

def draw_game_state():
    if not game_state:
        return {}

    food_count = 0
    my_ants = 0
    enemy_ants = 0
    visible_cells = len(game_state.get('map', []))
    total_points = game_state.get('points', 0)
    turn_number = game_state.get('turnNo', 0)

    # Рисуем гексы
    for hex_data in game_state.get('map', []):
        q, r = hex_data['q'], hex_data['r']
        hex_type = hex_data['type']
        
        hex_x = q * BASE_HEX_WIDTH
        if r % 2 == 1:
            hex_x += BASE_HEX_WIDTH / 2
        hex_y = r * BASE_HEX_VERTICAL_SPACING

        screen_x = WIDTH/2 + (hex_x - camera_x) * zoom_level
        screen_y = HEIGHT/2 + (hex_y - camera_y) * zoom_level
        
        draw_hexagon((screen_x, screen_y), BASE_HEX_SIZE * zoom_level, 
                    HEX_COLORS.get(hex_type, (50, 50, 150)), HEX_LINE_COLOR)

    # Рисуем ресурсы
    for food in game_state.get('food', []):
        food_count += 1

        q, r = food['q'], food['r']
        hex_x = q * BASE_HEX_WIDTH
        if r % 2 == 1:
            hex_x += BASE_HEX_WIDTH / 2
        hex_y = r * BASE_HEX_VERTICAL_SPACING

        screen_x = WIDTH/2 + (hex_x - camera_x) * zoom_level
        screen_y = HEIGHT/2 + (hex_y - camera_y) * zoom_level
        
        draw_food((screen_x, screen_y), BASE_HEX_SIZE * zoom_level, 
                 food['type'], food['amount'])

    # Рисуем муравьев
    for ant in game_state.get('ants', []):
        if ant.get('isEnemy'):
            enemy_ants += 1
        else:
            my_ants += 1

        q, r = ant['q'], ant['r']
        hex_x = q * BASE_HEX_WIDTH
        if r % 2 == 1:
            hex_x += BASE_HEX_WIDTH / 2
        hex_y = r * BASE_HEX_VERTICAL_SPACING

        screen_x = WIDTH/2 + (hex_x - camera_x) * zoom_level
        screen_y = HEIGHT/2 + (hex_y - camera_y) * zoom_level
        
        draw_ant((screen_x, screen_y), BASE_HEX_SIZE * zoom_level * 0.8, ant['type'])

        health_text = small_font.render(str(ant['health']), True, TEXT_COLOR)
        screen.blit(health_text, (screen_x - 10, screen_y - 20))

    # Рисуем муравейник
    for home in game_state.get('home', []):
        q, r = home['q'], home['r']
        hex_x = q * BASE_HEX_WIDTH
        if r % 2 == 1:
            hex_x += BASE_HEX_WIDTH / 2
        hex_y = r * BASE_HEX_VERTICAL_SPACING

        screen_x = WIDTH/2 + (hex_x - camera_x) * zoom_level
        screen_y = HEIGHT/2 + (hex_y - camera_y) * zoom_level
        
        pygame.draw.circle(screen, (255, 215, 0), (int(screen_x), int(screen_y)), 5)

    return {
        "turn": turn_number,
        "points": total_points,
        "my_ants": my_ants,
        "enemies": enemy_ants,
        "food": food_count,
        "cells": visible_cells
    }

def main():
    global camera_x, camera_y, zoom_level, dragging, last_mouse_pos, ws, running

    signal.signal(signal.SIGINT, graceful_exit)
    connect_websocket()

    clock = pygame.time.Clock()

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
        if keys[pygame.K_r]:  # Переподключение
            if ws:
                ws.close()
            connect_websocket()
            print("Попытка переподключения...")

        # Отрисовка
        if game_state:
            stats = draw_game_state()
            info_lines = [
                f"Turn: {stats['turn']}",
                f"Points: {stats['points']}",
                f"My ants: {stats['my_ants']}",
                f"Enemies: {stats['enemies']}",
                f"Food: {stats['food']}",
                f"Visible cells: {stats['cells']}"
            ]
            for i, line in enumerate(info_lines):
                info_text = font.render(line, True, TEXT_COLOR)
                screen.blit(info_text, (10, 80 + i * 20))
        else:
            no_conn_text = font.render("Нет соединения с сервером. Нажмите 'R' для переподключения.", True, (255, 0, 0))
            screen.blit(no_conn_text, (WIDTH // 2 - no_conn_text.get_width() // 2, HEIGHT // 2))

        mouse_pos = pygame.mouse.get_pos()
        hex_coords = screen_to_hex(mouse_pos)
        coord_text = font.render(f"Hex: {hex_coords[0]}, {hex_coords[1]}", True, COORD_TEXT_COLOR)
        screen.blit(coord_text, (mouse_pos[0] + 10, mouse_pos[1] + 10))

        if game_state:
            turn_text = font.render(f"Turn: {game_state.get('turnNo', 0)}", True, TEXT_COLOR)
            screen.blit(turn_text, (10, 10))

        camera_text = font.render(f"Camera: ({camera_x:.1f}, {camera_y:.1f}) Zoom: {zoom_level:.2f}x", True, TEXT_COLOR)
        screen.blit(camera_text, (10, 40))

        pygame.display.flip()
        clock.tick(60)

    graceful_exit(None, None)

if __name__ == "__main__":
    main()