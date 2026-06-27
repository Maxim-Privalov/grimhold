using Godot;
using System;

public partial class MusicManager : Node
{
	private AudioStreamPlayer _musicPlayer;
	private string _currentTrack = "";

	public override void _Ready()
	{
		_musicPlayer = new AudioStreamPlayer
		{
			Name = "MusicPlayer"
		};
		AddChild(_musicPlayer);
        
        _musicPlayer.Finished += OnMusicFinished;
	}

	// Обработчик окончания трека (для зацикливания)
	private void OnMusicFinished()
	{
		_musicPlayer.Play();
	}

	// Метод для смены музыки
	public void ChangeMusicByPath(string path)
	{
		// Проверяем, не играет ли уже этот трек
		if (_currentTrack == path && _musicPlayer.Playing)
		{
			GD.Print("Этот трек уже играет");
			return;
		}

		var music = GD.Load<AudioStream>(path);
		if (music == null)
		{
			GD.PrintErr($"Не удалось загрузить музыку: {path}");
			return;
		}

		// Меняем трек
		_musicPlayer.Stop(); // Останавливаем текущий
		_musicPlayer.Stream = music;
		_musicPlayer.Play();
		_currentTrack = path;
		
		GD.Print($"Музыка изменена на: {path}");
	}

	// Дополнительные полезные методы
	public void StopMusic()
	{
		_musicPlayer.Stop();
		GD.Print("Музыка остановлена");
	}

	public void ResumeMusic()
	{
		if (!_musicPlayer.Playing)
		{
			_musicPlayer.Play();
			GD.Print("Музыка возобновлена");
		}
	}

	public void SetVolume(float volume)
	{
		_musicPlayer.VolumeDb = Mathf.LinearToDb(volume);
	}

	public bool IsPlaying()
	{
		return _musicPlayer.Playing;
	}

	public override void _Process(double delta)
	{
		// Если ничего не делаете, удалите этот метод полностью
		// Или оставьте пустым
	}
}
