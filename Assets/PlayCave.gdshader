shader_type canvas_item;

uniform vec2 position;
uniform bool debug = true;

varying vec2 world_pos;
varying vec2 model_pos;

const float scale = 10.0;
const float innerRadius = 20.0;

void vertex()
{
	model_pos = (MODEL_MATRIX * vec4(0.0, 0.0, 1.0, 1.0)).xy;
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0, 1.0)).xy;
}

void fragment() {	
	vec4 sprite_color = texture(TEXTURE, UV);
	
	vec2 displacement = model_pos - position;
	if (displacement.y > -4.0) {
		displacement.y /= 2.0;
		displacement.y -= 10.0;
		float distancePxToPlayer = length(displacement);
		
		float _distance = max(0.0, distancePxToPlayer - innerRadius) / scale;
		
		sprite_color *= clamp(_distance * _distance, 0.2, 1.0);
	}
	
	COLOR = sprite_color * (debug ? 0.8 : 1.0);
}
