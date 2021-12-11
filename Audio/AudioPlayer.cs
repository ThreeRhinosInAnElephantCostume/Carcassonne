using Godot;
using System;
using System.Collections.Generic;

public class AudioPlayer : Node
{
	private AudioStreamPlayer _introMusic;
	private AudioStreamPlayer _tileRotationSound;
	private AudioStreamPlayer _tileRotationDisabledSound;
	private AudioStreamPlayer _tilePlacedSound;
	private AudioStreamPlayer _tileOverSpotSound;	
	
	private AudioStreamPlayer _meeplePlacedSound;	
	
	private float _lastMusicVolume;
	private float _lastSoundsVolume;

	private List<string> _songsList;
	private AudioStreamPlayer _currentSong;
	private int _currentSongIndex;
	private Random _random;
	private AudioStream _songStream;

	private float _muteLevel;
	
	public bool USER_PREFERENCES_AUTOPLAY;
	public float USER_PREFERENCES_MUSIC_VOLUME;
	public float USER_PREFERENCES_SOUNDS_VOLUME;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{		
		USER_PREFERENCES_AUTOPLAY = true;
		USER_PREFERENCES_MUSIC_VOLUME = 0.0f;
		USER_PREFERENCES_SOUNDS_VOLUME = 0.0f;
		
		
		
		_introMusic = (AudioStreamPlayer)GetNode("MusicPlayer").GetNode("IntroMusic");
		
		_tileRotationSound = (AudioStreamPlayer)GetNode("SoundsPlayer").GetNode("TileRotationAvailableSound");
		_tileRotationDisabledSound = (AudioStreamPlayer)GetNode("SoundsPlayer").GetNode("TileRotationDisabledSound");
		_tilePlacedSound = (AudioStreamPlayer)GetNode("SoundsPlayer").GetNode("TilePlacedSound");
		_tileOverSpotSound = (AudioStreamPlayer)GetNode("SoundsPlayer").GetNode("TileOverSpotSound");
		
		_meeplePlacedSound = (AudioStreamPlayer)GetNode("SoundsPlayer").GetNode("MeeplePlacedSound");
		
		_songsList = new List<string>(){
			"res://Audio/Music/2019-06-14_-_Warm_Light_-_David_Fesliyan.mp3",
			"res://Audio/Music/2019-07-26_-_Healing_Water_FesliyanStudios.com_-_David_Renda.mp3",
			"res://Audio/Music/2019-07-29_-_Elven_Forest_-_FesliyanStudios.com_-_David_Renda.mp3",
			"res://Audio/Music/2020-02-22_-_Relaxing_Green_Nature_-_David_Fesliyan.mp3",
			"res://Audio/Music/2020-06-18_-_Cathedral_Ambience_-_www.FesliyanStudios.com_David_Renda.mp3",
			"res://Audio/Music/2021-06-03_-_Irish_Sunset_-_www.FesliyanStudios.com.mp3",
			"res://Audio/Music/2021-06-03_-_Rolling_Hills_Of_Ireland_-_www.FesliyanStudios.com.mp3",
			"res://Audio/Music/bensound-epic.mp3",
			"res://Audio/Music/bensound-instinct.mp3",
		};

		_currentSong = (AudioStreamPlayer)GetNode("MusicPlayer").GetNode("CurrentSong");
		_currentSongIndex = 0 ;
		_random = new Random();
		_currentSong.Stream = _songStream;

		_muteLevel = -50.0f;

		_lastMusicVolume = USER_PREFERENCES_MUSIC_VOLUME;
		_lastSoundsVolume = USER_PREFERENCES_SOUNDS_VOLUME;
	}

	public void PlayNextSong(){
		if(USER_PREFERENCES_AUTOPLAY == true){
			if((_introMusic.Playing == true)||(_currentSongIndex >= _songsList.Count-1)){
				_currentSongIndex = 0; 	
			} else {
				_currentSongIndex++;
			}
			_songStream  = ResourceLoader.Load(_songsList[_currentSongIndex]) as AudioStream;
			_currentSong.Stream = _songStream;
			_currentSong.Play();
		}
	}	
	
	public void PlayIntroMusic(float startFrom = 0){
		_lastMusicVolume = _introMusic.VolumeDb;
		if(_introMusic.Playing == false){
			_introMusic.Play(startFrom);		
		}
	}

	public void StopIntroMusic(){
		_introMusic.Stop();
	}
	

	public void PlaySound(string namedStreamPlayer){
		AudioStreamPlayer _soundToPlay = (AudioStreamPlayer)GetNode("SoundsPlayer").GetNode(namedStreamPlayer);
		_soundToPlay.Play();
	}	

	public void SetAudioBusVolume(string audioBus, float value){
		float _volumeToSet = -_muteLevel*value*0.01f+_muteLevel;
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(audioBus),_volumeToSet);				
	}

	private void _setLastAudioVolume(string audioBus, float volume){
		switch(audioBus){
			case "Music":
				_lastMusicVolume = volume;
				break;
			case "Sounds":
				_lastSoundsVolume = volume;
				break;
			default:
				break;
		}
	}

	private float _getLastAudioVolume(string audioBus){
		switch(audioBus){
			case "Music":
				return _lastMusicVolume;
			case "Sounds":
				return _lastSoundsVolume;
			default:
				return 0;
		}
	}

	public void ToggleAudioBusVolume(string audioBus){
		float lastVolume = _getLastAudioVolume(audioBus);
		
		float currentSoundsVolume = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex(audioBus));
		if(currentSoundsVolume <= _muteLevel){
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(audioBus),lastVolume);
		} else {
			_setLastAudioVolume(audioBus,currentSoundsVolume);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(audioBus),_muteLevel);
		}
	}	
	// singnals handling
	private void _onIntroMusicFinished()
	{		
		PlayNextSong();
	}

	private void _onCurrentSongFinished()
	{
		PlayNextSong();
	}
}
