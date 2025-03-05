shader_type spatial;

uniform sampler2D noise;
uniform float height_scale = 0.5;

uniform sampler2D normalmap;

varying vec2 tex_position;

void vertex() {
  tex_position = VERTEX.xz / 2.0 + 0.5;
  float height = texture(noise, VERTEX.xz / 2.0 + 0.5).x;
  VERTEX.y += height * height_scale;
  //VERTEX.y += cos(VERTEX.x * 4.0) * sin(VERTEX.z * 4.0);
}

void fragment() {
  NORMAL_MAP = texture(normalmap, tex_position).xyz;
}