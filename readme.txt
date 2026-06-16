Team members: (names and student IDs)
* 0898503, Matthias Nijman
* 2541165 Tobias te Velde
* xxxxxxx Raphael Hurink

Tick the boxes below for the implemented features. Add a brief note only if necessary, e.g., if it's only partially working, or how to turn it on.

Formalities:
[X] This readme.txt
[X] Cleaned (no obj/bin folders)

Minimum requirements implemented:
[X] Camera: position and orientation controls, field of view in degrees
Controls: W = forward, A = left, S = backward, D = right, SpaceBar = up, Left_shift = down, Arrow Keys for rotation, scrolling for changing the fov

[X] Primitives: plane, sphere
[X] Lights: at least 2 point lights, additive contribution, shadows without "acne"
[X] Diffuse shading: (N.L), distance attenuation
[X] Phong shading: (R.V) or (N.H), exponent
[X] Diffuse color texture: only required on the plane primitive, image or procedural, (u,v) texture coordinates
[X] Mirror reflection: recursive
[X] Debug visualization: sphere primitives, rays (primary, shadow, reflected, refracted)

Bonus features implemented:
[X] Triangle primitives: must use the algorithm from the lectures, single triangles or meshes
[ ] Interpolated normals: only required on triangle primitives, 3 different vertex normals must be specified
[X] Spot lights: smooth falloff optional
[ ] Glossy reflections: not only of light sources but of other objects
[X] Anti-aliasing
[X] Parallelized
Method: parallel-for (for example: parallel-for, async tasks, or threads)
[X] Textures: on all implemented primitives
[ ] Bump or normal mapping: on all implemented primitives
[ ] Environment mapping: sphere or cube map, without intersecting actual sphere/cube/triangle primitives
[X] Refraction: also requires a reflected ray at every refractive surface, recursive
[ ] Area lights: soft shadows
[ ] Scene graph: nodes containing transformations, 3D models or primitives, and child nodes; flattening optional
[ ] Acceleration structure: bounding box or hierarchy, scene with 5000+ primitives
Performance comparison: ... (provide one measurement of speed/time with and without the acceleration structure)
[ ] GPU implementation
Method: ... (for example: fragment shader, compute shader, ILGPU, or CUDA)

Notes:
You turn on the debug screen by holding the K key
You turn on anti-aliasing by holding the A key
For the textures we have implemented a checkerboard texture on every primitive.
