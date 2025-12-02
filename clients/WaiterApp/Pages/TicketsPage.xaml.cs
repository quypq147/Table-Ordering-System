using System.Collections.ObjectModel;
using WaiterApp.Models;
using WaiterApp.Services;

namespace WaiterApp.Pages;

public partial class TicketsPage : ContentPage
{
    private readonly TableDto _table;
    private readonly KdsRealtimeService _kdsService;
    private readonly ObservableCollection<KitchenTicketDto> _tickets = new();

    public TicketsPage(TableDto table)
    {
        InitializeComponent();
        _table = table;
        _kdsService = App.KdsRealtimeService;

        TitleLabel.Text = $"Món c?a bàn {_table.Code}";
        TicketsCollection.ItemsSource = _tickets;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _kdsService.TicketsCreated += OnTicketsCreated;
        _kdsService.TicketChanged += OnTicketChanged;

        await _kdsService.StartAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _kdsService.TicketsCreated -= OnTicketsCreated;
        _kdsService.TicketChanged -= OnTicketChanged;
    }

    private void OnTicketsCreated(KitchenTicketDto[] tickets)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _tickets.Clear();

            foreach (var t in tickets.Where(t => t.TableName == _table.Code))
                _tickets.Add(t);
        });
    }

    private async void OnTicketChanged(KitchenTicketDto ticket)
    {
        if (ticket.TableName != _table.Code)
            return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var existing = _tickets.FirstOrDefault(x => x.Id == ticket.Id);
            if (existing != null)
            {
                var index = _tickets.IndexOf(existing);
                _tickets[index] = ticket;
            }
            else
            {
                _tickets.Add(ticket);
            }

            if (ticket.Status.Equals("Done", StringComparison.OrdinalIgnoreCase))
            {
                await DisplayAlertAsync("Món ?ã xong",
                    $"{ticket.ItemName} - SL: {ticket.Qty}",
                    "OK");
            }
        });
    }
}
