// NOTE: Shader automatically converted from Godot Engine 3.4.2.stable.mono's SpatialMaterial.

shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx,shadows_disabled,unshaded;
uniform vec4 albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular;
uniform float metallic;
uniform float roughness : hint_range(0,1);
uniform float point_size : hint_range(0,128);
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;

uniform vec4 primary_color : hint_color;
uniform vec4 secondary_color : hint_color;
uniform vec4 tertiary_color : hint_color;

uniform bool primary_enabled = true;
uniform bool secondary_enabled = true;
uniform bool tertiary_enabled = true;

uniform bool icon_enabled;
uniform bool icon_centered;
uniform bool icon_transform = true;
uniform sampler2D icon_texture;
uniform vec2 icon_scale;
uniform vec2 icon_offset;

uniform bool mask_enabled;
uniform sampler2D mask_texture;

uniform bool billboard_enabled = true;

uniform bool texture_enabled = false;
uniform sampler2DArray sprite_array;
uniform int sprite_index = -1;

uniform float background_alpha = 1.0;


void vertex() 
{
	if(billboard_enabled)
		MODELVIEW_MATRIX = INV_CAMERA_MATRIX * mat4(CAMERA_MATRIX[0],CAMERA_MATRIX[1],CAMERA_MATRIX[2],WORLD_MATRIX[3]);
}




void fragment() 
{
	vec3 channels = vec3(float(primary_enabled), float(secondary_enabled), float(tertiary_enabled));
	vec3 channels_inv = vec3(float(!primary_enabled), float(!secondary_enabled), float(!tertiary_enabled));
	vec2 base_uv = UV;
	vec3 base_albedo;
	float base_alpha;
	if(texture_enabled)
	{
		vec4 albedo_tex;
		if(sprite_index >= 0)
			albedo_tex = texture(sprite_array, vec3(base_uv, float(sprite_index)));
		else 
			albedo_tex = texture(texture_albedo,base_uv);
		float back_alpha = background_alpha;
		if(mask_enabled)
		{
			vec4 mask = texture(mask_texture, base_uv);
			vec3 back = albedo.rgb * (1.0-mask.a);
			back_alpha = clamp(background_alpha-mask.a, 0, 1);
			vec3 v = channels * mask.rgb;
			albedo_tex = vec4(v * albedo_tex.rgb + albedo_tex.rgb * channels_inv + back, albedo_tex.a);
		}
		else
		{
			albedo_tex = vec4((channels_inv * albedo_tex.rgb) +
			((
				channels.r * albedo_tex.r * primary_color.rgb +
				channels.g * albedo_tex.g * secondary_color.rgb +
				channels.b * albedo_tex.b * tertiary_color.rgb)), 
				albedo_tex.a
			);
		}
		base_albedo = albedo_tex.rgb + (albedo.rgb * (1.0 - albedo_tex.a));
		base_alpha = clamp(clamp(background_alpha, 0.0f, 1.0f) + albedo_tex.a, 0.0f, 1.0f);
	}
	else
	{
		base_albedo = albedo.rgb;
		base_alpha = background_alpha;
	} 
	if(icon_enabled)
	{
		vec2 icon_coords;
		if(icon_centered)
			icon_coords = ((base_uv-vec2(0.5f, 0.5f))/icon_scale) - icon_offset + vec2(0.5f, 0.5f);
		else
			icon_coords = (base_uv/icon_scale) - icon_offset;
		if(icon_coords.x < 0.0f || icon_coords.x > 1.0f || icon_coords.y < 0.0f || icon_coords.y > 1.0f)
		{
			ALBEDO = base_albedo;
			ALPHA = base_alpha;
		}
		else
		{
			vec4 icon_albedo = texture(icon_texture, icon_coords);
			if(icon_transform)
			{
				icon_albedo = vec4(icon_albedo.r * primary_color.rgb + 
					icon_albedo.g * secondary_color.rgb + 
					icon_albedo.b * tertiary_color.rgb,
					icon_albedo.a);
			}	
			ALBEDO = icon_albedo.rgb + (base_albedo * (1.0-icon_albedo.a));
			ALPHA = clamp(icon_albedo.a + clamp(base_alpha, 0, 1), 0, 1);
		}
	}
	else
	{
		ALBEDO = base_albedo;
		ALPHA = base_alpha;
	}
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
}
