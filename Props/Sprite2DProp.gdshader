shader_type canvas_item;

uniform vec4 primary_color : hint_color;
uniform vec4 secondary_color : hint_color;
uniform vec4 tertiary_color : hint_color;

uniform bool mask_enabled;
uniform sampler2D mask_texture;

uniform bool primary_enabled = true;
uniform bool secondary_enabled = true;
uniform bool tertiary_enabled = true;

uniform bool icon_enabled = false;
uniform bool icon_centered = true;
uniform vec2 icon_scale;
uniform vec2 icon_offset;
uniform sampler2D icon_texture;

void fragment()
{
	vec3 channels = vec3(float(primary_enabled), float(secondary_enabled), float(tertiary_enabled));
	vec3 channels_inv = vec3(float(!primary_enabled), float(!secondary_enabled), float(!tertiary_enabled));
	vec4 albedo_tex = texture(TEXTURE, UV);
	vec4 mask;
	if(mask_enabled)
		mask = texture(mask_texture, UV);
	else
		mask = vec4(1,1,1,1);
	vec3 v = channels * mask.rgb * mask.a;
	vec3 combcolor = v.r  * albedo_tex.r * primary_color.rgb + 
		v.g * albedo_tex.g * secondary_color.rgb + 
		v.b * albedo_tex.b * tertiary_color.rgb;
	albedo_tex = vec4(combcolor + albedo_tex.rgb * vec3(1.0f-v.r, 1.0f-v.g, 1.0f-v.b), albedo_tex.a);
	vec4 color = albedo_tex;
	if(icon_enabled)
	{
		vec2 icon_coords;
		if(icon_centered)
			icon_coords = ((UV-vec2(0.5f, 0.5f))/icon_scale) - icon_offset + vec2(0.5f, 0.5f);
		else
			icon_coords = (UV/icon_scale) - icon_offset;
		if(icon_coords.x < 0.0f || icon_coords.x > 1.0f || icon_coords.y < 0.0f || icon_coords.y > 1.0f)
		{
			color = albedo_tex;
		}
		else
		{
			vec4 icon_albedo = texture(icon_texture, icon_coords);
			icon_albedo = vec4(icon_albedo.r * primary_color.rgb + 
				icon_albedo.g * secondary_color.rgb + 
				icon_albedo.b * tertiary_color.rgb,
				icon_albedo.a);
			color = vec4(icon_albedo.rgb + (albedo_tex.rgb * (1.0-icon_albedo.a)),
				clamp(icon_albedo.a + clamp(albedo_tex.a, 0, 1), 0, 1));
		}
	}
	COLOR = color;
}