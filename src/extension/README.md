(1) Download NotoSansMono-Bold.ttf from https://fonts.google.com/noto/specimen/Noto+Sans+Mono , and move it into 'src/extension/Assets/Fonts/'

(2) Install Unity Editor 2022.3.38f1 or higher, and open 'src/extension/'

(3) import assets from Asset Store:

CITY package ver.1.0  
https://assetstore.unity.com/packages/3d/environments/urban/city-package-107224

Low Poly Cars ver.1.1  
https://assetstore.unity.com/packages/3d/vehicles/land/low-poly-cars-101798

(4) Modify aniso level:

Assets > POLYGON city pack > Meshes,textures > ways_FBX > Street_03.png  
Filter Mode Bilinear  
Aniso Level 10

Assets > POLYGON city pack > Meshes,textures > ways_FBX > Street_08.png  
Filter Mode Bilinear  
Aniso Level 5

(5) Open Game scenes and set Aspect ratio to 4:3

(6) Open Build Settings and switch platform to WebGL

(7) Bulid project in 'src/extension/selfdrivingcar/' and copy 'src/extension/selfdrivingcar/Build/' to 'src/scratch-gui/build/'
