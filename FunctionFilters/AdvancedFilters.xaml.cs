﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using FunctionFilters.Controls;
using FunctionFilters.Helpers;
using FunctionFilters.ImageManipulators;

namespace FunctionFilters
{
	/// <summary>
	/// Interaction logic for AdvancedFilters.xaml
	/// </summary>
	[ExcludeFromCodeCoverage]
	public sealed partial class AdvancedFilters : IDisposable
	{
		private readonly MainWindow _owner;
		private PlotViewModel _plotViewModel;
		private Bitmap _outputBitmap;

		public AdvancedFilters(MainWindow owner)
		{
			_owner = owner;
			Owner = owner;
			_plotViewModel = new PlotViewModel();
			DataContext = _plotViewModel;
			_outputBitmap = new Bitmap(_owner.SourceBitmap);
			InitializeComponent();
		}

		public void Dispose()
		{
			_outputBitmap.Dispose();
		}

		private void AdvancedFilters_Loaded(object sender, RoutedEventArgs e)
		{
			AdvancedFunction.DefaultTrackerTemplate = null;
		}

		private void ApplyFunction()
		{
			var colorMap = GenerateColorMap();
			var selectedChannel = (ColorChannel)ChannelComboBox.SelectedItem;
			var outputBitmap = _outputBitmap.ApplyManipulation(colorMap, selectedChannel);

			_outputBitmap = outputBitmap;
			_owner.OutputPhoto.Background = outputBitmap.CreateImageBrush();
		}

		private int[] GenerateColorMap()
		{
			var colorMap = new int[256];
			var pointsList = _plotViewModel.Points;
			for (int i = 0; i < pointsList.Count - 1; i++)
			{
				// y = ax + b
				// y1 = ax1 + b

				// y - ax = y1 - ax1
				// a = (y1 - y)/(x1 - x)
				// b = y - a * x

				double a = (pointsList[i + 1].Y - pointsList[i].Y) / (pointsList[i + 1].X - pointsList[i].X);
				double b = pointsList[i].Y - a * pointsList[i].X;

				for (int j = (int)pointsList[i].X; j <= (int)pointsList[i + 1].X; j++)
				{
					colorMap[j] = (a * j + b).ToRgb();
				}
			}

			return colorMap;
		}

		private void ChannelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_plotViewModel.Points.Count == 2)
				return;

			ApplyFunction();
			_plotViewModel = new PlotViewModel();
			DataContext = _plotViewModel;
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			ApplyFunction();
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}