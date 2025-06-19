shader_type canvas_item;

uniform vec4 input_colour;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	if (input_colour == vec4(1.0, 1.0, 1.0, 1.0)){}
	else{
		vec4 original_colour = texture(TEXTURE, UV);
		float strength = input_colour.a;
		float avg = (original_colour.r + original_colour.g + original_colour.b)/float(3);
		vec4 greyscale = vec4(avg, avg, avg, original_colour.a);

		vec4 tinted = input_colour*greyscale;
		vec4 blend_colour = mix(original_colour, tinted, strength);

		vec4 modulate_colour = input_colour*original_colour;
		if (dot(modulate_colour, modulate_colour) > dot(blend_colour, blend_colour)){
			COLOR = modulate_colour;
		}else{
			COLOR = blend_colour;

		}

	}
	//COLOR = mix(original_colour,blend_colour, strength);
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
