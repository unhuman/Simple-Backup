#!/usr/bin/env python3
"""
Simple Backup Icon Generator
Hard drive (top-left) + USB stick (bottom-right) with blue arrow between them
Properly saves all sizes as 32-bit RGBA in the ICO file
"""

from PIL import Image, ImageDraw
import os
import math

def create_icon_image(size):
    """Create icon image at specified size"""
    # Create image at the target size directly to avoid scaling artifacts
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, 'RGBA')
    
    # Scale factors relative to 256x256 base
    scale = size / 256.0
    
    # Colors
    LIGHT_GRAY = (200, 200, 200, 255)
    DARK_GRAY = (100, 100, 100, 255)
    MID_GRAY = (120, 120, 120, 255)
    BLACK = (20, 20, 20, 255)
    BLUE = (0, 102, 204, 255)
    GOLD = (218, 165, 32, 255)
    
    # Scale all coordinates
    def sc(val):
        return int(val * scale)
    
    # ===== HARD DRIVE (top-left) =====
    hd_x1, hd_y1 = sc(10), sc(12)
    hd_x2, hd_y2 = sc(140), sc(125)
    
    # Main body
    draw.rectangle([hd_x1, hd_y1, hd_x2, hd_y2], fill=LIGHT_GRAY, outline=BLACK, width=max(1, sc(2)))
    
    # Top edge (3D)
    draw.rectangle([hd_x1, hd_y1, hd_x2, hd_y1 + max(1, sc(14))], fill=DARK_GRAY)
    
    # Vent lines
    draw.line([hd_x1 + sc(6), hd_y1 + sc(28), hd_x2 - sc(6), hd_y1 + sc(28)], fill=BLACK, width=max(1, sc(2)))
    draw.line([hd_x1 + sc(6), hd_y1 + sc(42), hd_x2 - sc(6), hd_y1 + sc(42)], fill=BLACK, width=max(1, sc(2)))
    draw.line([hd_x1 + sc(6), hd_y1 + sc(56), hd_x2 - sc(6), hd_y1 + sc(56)], fill=BLACK, width=max(1, sc(2)))
    
    # Platter
    platter_cx = (hd_x1 + hd_x2) // 2
    platter_cy = (hd_y1 + hd_y2) // 2
    platter_r = max(1, sc(15))
    draw.ellipse([platter_cx - platter_r, platter_cy - platter_r, 
                  platter_cx + platter_r, platter_cy + platter_r], 
                 fill=BLACK, outline=MID_GRAY, width=max(1, sc(2)))
    
    # ===== USB STICK (bottom-right) =====
    usb_x1, usb_y1 = sc(155), sc(150)
    usb_x2, usb_y2 = sc(245), sc(240)
    
    # Main USB body
    draw.rounded_rectangle([usb_x1 + sc(10), usb_y1 + sc(15), usb_x2, usb_y2], 
                          radius=max(1, sc(12)), fill=BLACK)
    
    # USB connector
    connector_x1, connector_y1 = usb_x1 + sc(12), usb_y1
    connector_x2, connector_y2 = usb_x2 - sc(12), usb_y1 + sc(15)
    draw.rectangle([connector_x1, connector_y1, connector_x2, connector_y2], 
                   fill=GOLD, outline=BLACK, width=1)
    
    # Connector details
    draw.rectangle([connector_x1 + sc(5), connector_y1 + sc(3), 
                   connector_x1 + sc(10), connector_y1 + sc(10)], fill=BLACK)
    draw.rectangle([connector_x2 - sc(10), connector_y1 + sc(3), 
                   connector_x2 - sc(5), connector_y1 + sc(10)], fill=BLACK)
    
    # Status LED
    draw.ellipse([usb_x1 + sc(18), usb_y1 + sc(42), usb_x1 + sc(30), usb_y1 + sc(54)], fill=BLUE)
    
    # ===== BLUE ARROW =====
    arrow_start_x = platter_cx
    arrow_start_y = platter_cy
    arrow_end_x = (usb_x1 + usb_x2) // 2
    arrow_end_y = (usb_y1 + usb_y2) // 2
    
    # Arrow line
    draw.line([arrow_start_x, arrow_start_y, arrow_end_x, arrow_end_y], fill=BLUE, width=max(1, sc(9)))
    
    # Arrow head
    dx = arrow_end_x - arrow_start_x
    dy = arrow_end_y - arrow_start_y
    length = math.sqrt(dx*dx + dy*dy)
    
    if length > 0:
        ux = dx / length
        uy = dy / length
        px = -uy
        py = ux
        
        head_size = sc(18)
        point1_x = arrow_end_x - ux * head_size
        point1_y = arrow_end_y - uy * head_size
        
        point2_x = point1_x + px * (head_size * 0.7)
        point2_y = point1_y + py * (head_size * 0.7)
        
        point3_x = point1_x - px * (head_size * 0.7)
        point3_y = point1_y - py * (head_size * 0.7)
        
        draw.polygon([(arrow_end_x, arrow_end_y), (point2_x, point2_y), (point3_x, point3_y)], fill=BLUE)
    
    return img

# ===== GENERATE AND SAVE =====
sizes = [256, 128, 64, 48, 32, 16]
output = 'backup-icon.ico'

# Create all sizes
icon_images = [create_icon_image(size) for size in sizes]

# Save as ICO - use the largest as the main image and pass all as sizes
icon_images[0].save(
    output,
    format='ICO',
    sizes=[(s, s) for s in sizes],
    append_images=icon_images[1:] if len(icon_images) > 1 else []
)

print(f"? Icon saved: {output}")
print(f"? Sizes: {', '.join([f'{s}x{s}' for s in sizes])}")
print(f"? All images created at native size (no scaling artifacts)")
print(f"? Design: Hard drive (top-left) ? USB stick (bottom-right) with blue arrow")
