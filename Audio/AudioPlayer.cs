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
		try{
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

			_muteLevel = -55.0f;

			_lastMusicVolume = USER_PREFERENCES_MUSIC_VOLUME;
			_lastSoundsVolume = USER_PREFERENCES_SOUNDS_VOLUME;
		} catch (NullReferenceException e){
			System.Diagnostics.Debug.WriteLine("Exception caught!. There is no such a node. "+e.Message);
		} catch (Exception e){
			System.Diagnostics.Debug.WriteLine("AudioPlayer Exception: "+e.Message);
		}
	}

	public void PlayNextSong(){
		try{
			if(_songsList.Count < 1)
				throw new OperationCanceledException("There are no songs found on the playlist.");

			if(USER_PREFERENCES_AUTOPLAY == true){
				if((_introMusic.Playing == true)||(_currentSongIndex >= _songsList.Count -1)){
					_currentSongIndex = 0; 	
				} else {
					_currentSongIndex++;
				}
				_songStream  = ResourceLoader.Load(_songsList[_currentSongIndex]) as AudioStream;
				_currentSong.Stream = _songStream;
				_currentSong.Play();
			}
		} catch(NullReferenceException e){
			System.Diagnostics.Debug.WriteLine("AudioPlayer Exception: "+e.Message);
		} catch(ArgumentOutOfRangeException e){
			System.Diagnostics.Debug.WriteLine("AudioPlayer Excetpion! There is no song with given index! Skipping to first song...");
			System.Diagnostics.Debug.WriteLine("EXCEPTION MESSAGE: " + e.Message);
			_currentSongIndex = 0;
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
		try{
			AudioStreamPlayer _soundToPlay = (AudioStreamPlayer)GetNode("SoundsPlayer").GetNode(namedStreamPlayer);
			_soundToPlay.Play();
		} catch(NullReferenceException e){
			System.Diagnostics.Debug.WriteLine("Exception caught!. Can't find sound called: "+namedStreamPlayer);
			System.Diagnostics.Debug.WriteLine("Exception message: "+e.Message);
		}
	}	

	public void SetAudioBusVolume(string audioBus, float value){
		float volumeToSet = -_muteLevel*value*0.01f+_muteLevel;
		volumeToSet = verifyVolumeLevel(volumeToSet);
		try{
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(audioBus),volumeToSet);				
		} catch(NullReferenceException e){
			System.Diagnostics.Debug.WriteLine("Exception caught!. Can't find Audio Bus named: "+audioBus);
			System.Diagnostics.Debug.WriteLine("Exception message: "+e.Message);
		}
	}

	private float verifyVolumeLevel(float volume){
		if(volume < _muteLevel)
			return _muteLevel;
		if(volume > 0)
			return 0.0f;
		return volume;
	}

	public void ToggleAudioBusVolume(string audioBus){
		try{
			int audioBusIndex = AudioServer.GetBusIndex(audioBus);
			if(audioBusIndex < 0)
				throw new ArgumentException("AudioPlayer Exception: Unknown AudioBus '" + audioBus +"'");
			if(AudioServer.IsBusMute(audioBusIndex))
				AudioServer.SetBusMute(audioBusIndex,false);
			else
				AudioServer.SetBusMute(audioBusIndex,true);

		} catch (Exception e) {
			System.Diagnostics.Debug.WriteLine(e.Message);
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
