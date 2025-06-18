shader_type canvas_item;

uniform vec4 input_colour : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float strength;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	if (input_colour == vec4(1.0, 1.0, 1.0, 1.0)){
	}else{
		vec4 original_colour = texture(TEXTURE, UV);
	vec4 blend_colour = vec4(input_colour.r, input_colour.g, input_colour.b, original_colour.a);
	COLOR = mix(COLOR, blend_colour, .5);
	}
	
	
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
