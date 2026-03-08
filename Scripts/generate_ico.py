import os
import sys
import subprocess

def install_deps():
    print("Installing dependencies (cairosvg, pillow)...")
    subprocess.check_call([sys.executable, "-m", "pip", "install", "cairosvg", "pillow"])

def generate():
    try:
        import cairosvg
        from PIL import Image
        from io import BytesIO
    except ImportError:
        install_deps()
        import cairosvg
        from PIL import Image
        from io import BytesIO

    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.dirname(script_dir)
    svg_path = os.path.join(project_root, 'UnrealBinaryBuilder.Avalonia', 'Assets', 'app_icon.svg')
    icon_path = os.path.join(project_root, 'UnrealBinaryBuilder.Avalonia', 'Assets', 'app_icon.ico')

    if not os.path.exists(svg_path):
        print(f"Error: {svg_path} not found.")
        sys.exit(1)

    # Standard ICO sizes
    target_sizes = [256, 48, 32, 16] # Try descending order
    images = []

    print(f"Generating icon from {svg_path}...")
    
    for size in target_sizes:
        try:
            png_data = cairosvg.svg2png(url=svg_path, output_width=size, output_height=size)
            if not png_data or len(png_data) < 100:
                raise ValueError(f"Generated PNG for size {size} is empty or too small.")
            
            img = Image.open(BytesIO(png_data))
            # We must convert to RGBA for ICO
            if img.mode != 'RGBA':
                img = img.convert('RGBA')
            
            images.append(img)
            print(f"  - Rendered {size}x{size} ({len(png_data)} bytes)")
        except Exception as e:
            print(f"Error rendering size {size}: {e}")
            sys.exit(1)

    if not images:
        print("Error: No images were generated.")
        sys.exit(1)

    try:
        # Save ICO including all frames
        # The first image is the 'main' one, others are appended as frames
        images[0].save(icon_path, format='ICO', append_images=images[1:])
        
        file_size = os.path.getsize(icon_path)
        # Expected size for 256+48+32+16 should be > 20KB if uncompressed, 
        # but 256 is often PNG-compressed in ICO. 
        if file_size < 5000: # 5KB is a safer floor for a 256px icon
             raise ValueError(f"Generated ICO file is suspiciously small ({file_size} bytes).")
             
        print(f"Successfully created {icon_path} ({file_size} bytes)")
    except Exception as e:
        print(f"Error saving ICO file: {e}")
        sys.exit(1)

if __name__ == "__main__":
    generate()
