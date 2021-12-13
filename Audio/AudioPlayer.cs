using Godot;
using System;
using System.Linq;
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
	private string[] _allowedMusicTypes;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{				
		USER_PREFERENCES_AUTOPLAY = true;
		USER_PREFERENCES_MUSIC_VOLUME = 0.0f;
		USER_PREFERENCES_SOUNDS_VOLUME = 0.0f;		
		
		_introMusic = GetNode("MusicPlayer").GetNode<AudioStreamPlayer>("IntroMusic");
		
		_tileRotationSound = GetNode("SoundsPlayer").GetNode<AudioStreamPlayer>("TileRotationAvailableSound");
		_tileRotationDisabledSound = GetNode("SoundsPlayer").GetNode<AudioStreamPlayer>("TileRotationDisabledSound");
		_tilePlacedSound = GetNode("SoundsPlayer").GetNode<AudioStreamPlayer>("TilePlacedSound");
		_tileOverSpotSound = GetNode("SoundsPlayer").GetNode<AudioStreamPlayer>("TileOverSpotSound");
				
		_meeplePlacedSound = GetNode("SoundsPlayer").GetNode<AudioStreamPlayer>("MeeplePlacedSound");
		
		_allowedMusicTypes = new string[] {"mp3", "ogg", "wav"};

		_songsList = ListMimeFilesInDirectory("res://Audio/Music",_allowedMusicTypes);

		_currentSong = GetNode("MusicPlayer").GetNode<AudioStreamPlayer>("CurrentSong");
		_currentSongIndex = 0 ;
		_random = new Random();
		_currentSong.Stream = _songStream;

		_muteLevel = -55.0f;

		_lastMusicVolume = USER_PREFERENCES_MUSIC_VOLUME;
		_lastSoundsVolume = USER_PREFERENCES_SOUNDS_VOLUME;				
	}

	public static List<string> ListMimeFilesInDirectory(string directoryPath, string[] mimeTypes)
	{
		
		Directory directory = new Directory();
		string fileName = "";
		List<string> filesList = new List<string>(){};
		if (directory.Open(directoryPath) == Error.Ok)
		{
			directory.ListDirBegin();
			fileName = directory.GetNext();
			while(fileName != "")
			{
				if(directory.CurrentIsDir() == false)				
				{
					string fileExtension = System.IO.Path.GetExtension(fileName).Replace(".", "");
					if(mimeTypes.Contains(fileExtension))
						filesList.Add(directoryPath+"/"+fileName);											
				}					
				fileName = directory.GetNext();
			}
		}
		else
			GD.Print("An error occurred while opening dir " + directoryPath);
		return filesList;
	}

	public void PlayNextSong()
	{		
		if(USER_PREFERENCES_AUTOPLAY == true)
		{
			if((_introMusic.Playing == true)||(_currentSongIndex >= _songsList.Count -1))
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
	
	public void PlaySound(string namedStreamPlayer)
	{
		AudioStreamPlayer _soundToPlay = GetNode("SoundsPlayer").GetNode<AudioStreamPlayer>(namedStreamPlayer);
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
		if(volume < _muteLevel)
			return _muteLevel;
		if(volume > 0)
			return 0.0f;
		return volume;
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
