shader_type canvas_item;

void fragment() {
	COLOR.xyz = vec3(0.0, 0.7, 1.0);
	
	float a = texture(TEXTURE, SCREEN_UV + vec2(0.0, SCREEN_PIXEL_SIZE.y)).a;
	a *= texture(TEXTURE, SCREEN_UV + vec2(0.0, -SCREEN_PIXEL_SIZE.y)).a;
	a *= texture(TEXTURE, SCREEN_UV + vec2(SCREEN_PIXEL_SIZE.x, 0.0)).a;
	a *= texture(TEXTURE, SCREEN_UV + vec2(-SCREEN_PIXEL_SIZE.x, 0.0)).a;
	//a *= texture(TEXTURE, SCREEN_UV + SCREEN_PIXEL_SIZE.xy).a;
	//a *= texture(TEXTURE, SCREEN_UV - SCREEN_PIXEL_SIZE.xy).a;
	//a *= texture(TEXTURE, SCREEN_UV + vec2(-SCREEN_PIXEL_SIZE.x, SCREEN_PIXEL_SIZE.y)).a;
	//a *= texture(TEXTURE, SCREEN_UV + vec2(SCREEN_PIXEL_SIZE.x, -SCREEN_PIXEL_SIZE.y)).a;

	COLOR.a = max(texture(TEXTURE, SCREEN_UV).a - a, 0.05 * texture(TEXTURE, SCREEN_UV).a);
}