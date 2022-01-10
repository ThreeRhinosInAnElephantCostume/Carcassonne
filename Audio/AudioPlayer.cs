using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public class AudioPlayer : Node
{
	AudioStreamPlayer _introMusic;
	AudioStreamPlayer _mainMenuMusic;
	AudioStreamPlayer _tileRotationSound;
	AudioStreamPlayer _tileRotationDisabledSound;
	AudioStreamPlayer _tilePlacedSound;
	AudioStreamPlayer _tileOverSpotSound;	
	
	AudioStreamPlayer _meeplePlacedSound;	
	
	float _lastMusicVolume;
	float _lastSoundsVolume;

	List<string> _songsList;
	AudioStreamPlayer _currentSong;
	int _currentSongIndex;
	Random _random;
	AudioStream _songStream;
	float _muteLevel;
	string[] _allowedMusicTypes;
	
	public bool USER_PREFERENCES_AUTOPLAY;
	public float USER_PREFERENCES_MUSIC_VOLUME;
	public float USER_PREFERENCES_SOUNDS_VOLUME;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{				
		USER_PREFERENCES_AUTOPLAY = true;
		USER_PREFERENCES_MUSIC_VOLUME = 0.0f;
		USER_PREFERENCES_SOUNDS_VOLUME = 0.0f;		
		
		_introMusic = GetNode<AudioStreamPlayer>("MusicPlayer/IntroMusic");
		_mainMenuMusic = GetNode<AudioStreamPlayer>("MusicPlayer/MainMenuMusic");
		
		_tileRotationSound = GetNode<AudioStreamPlayer>("SoundsPlayer/TileRotationAvailableSound");
		_tileRotationDisabledSound = GetNode<AudioStreamPlayer>("SoundsPlayer/TileRotationDisabledSound");
		_tilePlacedSound = GetNode<AudioStreamPlayer>("SoundsPlayer/TilePlacedSound");
		_tileOverSpotSound = GetNode<AudioStreamPlayer>("SoundsPlayer/TileOverSpotSound");
				
		_meeplePlacedSound = GetNode<AudioStreamPlayer>("SoundsPlayer/MeeplePlacedSound");
		
		_allowedMusicTypes = new string[] {"mp3", "ogg", "wav"};

		_songsList = ListMimeFilesInDirectory("res://Audio/Music",_allowedMusicTypes);

		_currentSong = GetNode<AudioStreamPlayer>("MusicPlayer/CurrentSong");
		_currentSongIndex = 0 ;
		_random = new Random();
		_currentSong.Stream = _songStream;

		_muteLevel = -55.0f;

		_lastMusicVolume = USER_PREFERENCES_MUSIC_VOLUME;
		_lastSoundsVolume = USER_PREFERENCES_SOUNDS_VOLUME;				
	}

	public static List<string> ListMimeFilesInDirectory(string directoryPath, string[] mimeTypes)
	{
		
		return Utils.ListDirectoryFiles(directoryPath).FindAll(path => mimeTypes.Any(ext => path.EndsWith(ext)));
	}

	public void PlayNextSong()
	{		
		if(USER_PREFERENCES_AUTOPLAY)
		{
			if((_introMusic.Playing)||(_currentSongIndex >= _songsList.Count -1))
				_currentSongIndex = 0; 	
			else
				_currentSongIndex++;
			_songStream  = ResourceLoader.Load(_songsList[_currentSongIndex]) as AudioStream;
			_currentSong.Stream = _songStream;			
			_currentSong.Play();
		}		
	}	
	
	public void PlayIntroMusic(float startFrom = 0)
	{
		_lastMusicVolume = _introMusic.VolumeDb;
		if(_introMusic.Playing == false)
			_introMusic.Play(startFrom);		
	}
	
	public void StopIntroMusic()
	{
		_introMusic.Stop();
	}
	
	public void PlayMainMenuMusic(float startFrom = 0)
	{
		_lastMusicVolume = _mainMenuMusic.VolumeDb;
		if(_mainMenuMusic.Playing == false)
			_mainMenuMusic.Play(startFrom);		
	}
	
	public void StopMainMenuMusic()
	{
		_mainMenuMusic.Stop();
	}
	
	public void PlaySound(string namedStreamPlayer)
	{
		AudioStreamPlayer _soundToPlay = GetNode<AudioStreamPlayer>("SoundsPlayer/"+namedStreamPlayer);
		_soundToPlay.Play();		
	}	

	public void SetAudioBusVolume(string audioBus, float value)
	{
		float volumeToSet = -_muteLevel*value*0.01f+_muteLevel;	
		volumeToSet = verifyVolumeLevel(volumeToSet);				
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(audioBus),volumeToSet);						
	}

	private float verifyVolumeLevel(float volume)
	{
		return Math.Clamp(volume, _muteLevel, 0);
	}

	public void ToggleAudioBusVolume(string audioBus)
	{		
		int audioBusIndex = AudioServer.GetBusIndex(audioBus);

		if(AudioServer.IsBusMute(audioBusIndex))
			AudioServer.SetBusMute(audioBusIndex,false);
		else
			AudioServer.SetBusMute(audioBusIndex,true);		
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
