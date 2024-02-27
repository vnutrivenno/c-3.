using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using NAudio.Wave;

namespace AudioPlayer
{
    public partial class MainWindow : Window
    {
        private List<string> audioFiles = new List<string>();
        private bool isPlaying = false;
        private bool isRepeating = false;
        private bool isShuffling = false;
        private CancellationTokenSource cancellationTokenSource;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                audioFiles.AddRange(openFileDialog.FileNames);
                PlayFirstSong();
            }
        }

        private async void PlayFirstSong()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            string firstSong = audioFiles.FirstOrDefault();
            if (firstSong != null)
            {
                await PlayAudio(firstSong, cancellationTokenSource.Token);
            }
        }

        private async void PlayAudio(string audioFilePath, CancellationToken cancellationToken)
        {
            isPlaying = true;
            playPauseButton.Content = "Pause";
            mediaElement.Source = new Uri(audioFilePath);

            try
            {
                mediaElement.Play();
                while (mediaElement.Position < mediaElement.NaturalDuration.TimeSpan)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await Task.Delay(1000);
                    UpdateTimeRemaining();
                }

                if (isRepeating && !cancellationToken.IsCancellationRequested)
                {
                    await PlayAudio(audioFilePath, cancellationToken);
                }
                else if (!cancellationToken.IsCancellationRequested)
                {
                    PlayNextSong();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing audio: {ex.Message}");
            }
        }

        private void UpdateTimeRemaining()
        {
            TimeSpan remaining = mediaElement.NaturalDuration.TimeSpan - mediaElement.Position;
            timeRemainingTextBlock.Text = $"{remaining:mm\\:ss} remaining";
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Position = TimeSpan.FromSeconds(positionSlider.Value);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = volumeSlider.Value;
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                mediaElement.Pause();
                playPauseButton.Content = "Play";
                isPlaying = false;
            }
            else
            {
                mediaElement.Play();
                playPauseButton.Content = "Pause";
                isPlaying = true;
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            int currentIndex = audioFiles.IndexOf(mediaElement.Source.LocalPath);
            int previousIndex = currentIndex - 1;
            if (previousIndex >= 0)
            {
                string previousSong = audioFiles[previousIndex];
                PlayAudio(previousSong, CancellationToken.None);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }

        private void PlayNextSong()
        {
            int currentIndex = audioFiles.IndexOf(mediaElement.Source.LocalPath);
            int nextIndex;
            if (isShuffling)
            {
                nextIndex = random.Next(audioFiles.Count);
            }
            else
            {
                nextIndex = (currentIndex + 1) % audioFiles.Count;
            }

            string nextSong = audioFiles[nextIndex];
            PlayAudio(nextSong, CancellationToken.None);
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            isRepeating = !isRepeating;
            repeatButton.Content = isRepeating ? "Repeat On" : "Repeat Off";
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            isShuffling = !isShuffling;
            shuffleButton.Content = isShuffling ? "Shuffle On" : "Shuffle Off";
        }
    }
}
