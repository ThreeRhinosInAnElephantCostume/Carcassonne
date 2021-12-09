using Godot;
using System;

public class AudioPlayer : Node
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private AudioStreamPlayer _introMusic;
	private AudioStreamPlayer _tileRotationSound;
	private AudioStreamPlayer _tileRotationDisabledSound;
	private AudioStreamPlayer _tilePlacedSound;

	private AudioStreamPlayer _tileOverSpotSound;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{		
		_introMusic = (AudioStreamPlayer)GetNode("IntroMusic");
		_tileRotationSound = (AudioStreamPlayer)GetNode("TileRotationAvailableSound");
		_tileRotationDisabledSound = (AudioStreamPlayer)GetNode("TileRotationDisabledSound");
		_tilePlacedSound = (AudioStreamPlayer)GetNode("TilePlacedSound");
		_tileOverSpotSound = (AudioStreamPlayer)GetNode("TileOverSpotSound");
		
	}	
	
	public void PlayIntroMusic(float startFrom = 0, float volume = 0){
		_introMusic.SetVolumeDb(volume);
		if(_introMusic.Playing == false){
			_introMusic.Play(startFrom);		
		}
	}
	public void SetIntroVolume(float volume){
		_introMusic.SetVolumeDb(volume);
	}
	
	public void PlayTilePlacedSound(float volume = 0){
		_tilePlacedSound.SetVolumeDb(volume);
		if(_tilePlacedSound.Playing == false){
			_tilePlacedSound.Play();
		}
	}
	
	public void PlayTileRotationSound(float volume = 0){
		_tileRotationSound.SetVolumeDb(volume);
		_tileRotationSound.Play();
	}
	
	public void PlayTileRotationDisabledSound(float volume = 0){
		_tileRotationDisabledSound.SetVolumeDb(volume);
		_tileRotationDisabledSound.Play();
	}
	
	public void PlayTileOverSpotSound(float volume = 0){
		_tileOverSpotSound.SetVolumeDb(volume);
		_tileOverSpotSound.Play();
	}
}
