using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Graphium.Controls;
using Graphium.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using Graphium.Interfaces;

namespace Graphium.Behaviors
{
    public class VisualizerGridBehavior : Behavior<Grid>
    {
        public ObservableCollection<ISignalVisualizerViewModel> Visualizers
        {
            get => (ObservableCollection<ISignalVisualizerViewModel>)GetValue(VisualizersProperty);
            set => SetValue(VisualizersProperty, value);
        }

        public static readonly DependencyProperty VisualizersProperty =
            DependencyProperty.Register(
                nameof(Visualizers),
                typeof(ObservableCollection<ISignalVisualizerViewModel>),
                typeof(VisualizerGridBehavior),
                new PropertyMetadata(null, OnVisualizersChanged));

        private static void OnVisualizersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (VisualizerGridBehavior)d;

            if (e.OldValue is ObservableCollection<ISignalVisualizerViewModel> oldCol)
                oldCol.CollectionChanged -= behavior.OnCollectionChanged;

            if (e.NewValue is ObservableCollection<ISignalVisualizerViewModel> newCol)
                newCol.CollectionChanged += behavior.OnCollectionChanged;

            behavior.RebuildGrid();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildGrid();
        }

        private void RebuildGrid()
        {
            if (AssociatedObject == null || Visualizers == null) return;

            var grid = AssociatedObject;
            grid.Children.Clear();
            grid.RowDefinitions.Clear();

            for (int i = 0; i < Visualizers.Count; i++)
            {
                if (i > 0)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });
                    var splitter = new GridSplitter
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Height = 2,
                        Cursor = Cursors.SizeNS
                    };
                    Grid.SetRow(splitter, grid.RowDefinitions.Count - 1);
                    grid.Children.Add(splitter);
                }

                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                UserControl view = Visualizers[i] switch
                {
                    NumericSignalViewModel => new NumericSignalControl(),
                    TextSignalViewModel => new TextSignalControl(),
                    _ => throw new InvalidOperationException()
                };

                view.DataContext = Visualizers[i];
                Grid.SetRow(view, grid.RowDefinitions.Count - 1);
                grid.Children.Add(view);
            }
        }

        protected override void OnDetaching()
        {
            if (Visualizers != null)
                Visualizers.CollectionChanged -= OnCollectionChanged;
            base.OnDetaching();
        }
    }
}