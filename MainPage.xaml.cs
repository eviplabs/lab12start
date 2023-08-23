using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.ObjectModel;
using System.Threading;

namespace LAB12_MAUI_ThreadingLab;

public partial class MainPage : ContentPage
{
    public ObservableCollection<string> Events { get; set; } = new ObservableCollection<string>();

	public MainPage()
	{
        this.BindingContext = this;
        InitializeComponent();
        AddEvent("App started...");
    }

    public void AddEvent(string text)
    {
        Events.Add(text);
        // ScrollTo needed as at time of implementation, CollectionView.ItemsUpdatingScrollMode has bugs.
        EventListView.ScrollTo(Events.Last(), ScrollToPosition.End);    // Note: this can only be called from the UI thread
    }

    private void Blocker_Clicked(object sender, EventArgs e)
    {
        AddEvent("UI blocking task started.");
        Task.Delay(3000).Wait();
        AddEvent("UI blocking task finished.");
    }

    private void Start_Clicked(object sender, EventArgs e)
    {
        AddEvent("Start clicked");
        var progressReporter = new Progress<int>(percent => this.ProgressBar1.Progress = percent / 100.0);
        var slowBackgroundProcessor = new SlowBackgroundProcessor();
        slowBackgroundProcessor.DoIt(progressReporter);
        AddEvent("Finished");
    }

    class SlowBackgroundProcessor
    {
        private ObservableCollection<string> events;
        private MainPage page;

        public SlowBackgroundProcessor(ObservableCollection<string> events = null, MainPage page = null)
        {
            this.events = events;
            this.page = page;
        }

        public void DoIt(IProgress<int> progress)
        {
            for (int i = 0; i <= 100; i += 10)
            {
                Task.Delay(500).Wait();
                progress.Report(i);
            }
        }
    }
}

