using Vertiq.DB;
using Vertiq.Messaging;
using Clockly.Models;
using Fluxor;
using Vertiq.Actions;
using Microsoft.AspNetCore.Components;

namespace Clockly.Pages.TimeTracking;

public partial class EnterTime
{
    public sealed record AddTimeViewModel
    {
        public string Description { get; init; } = "";

        public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.Today);

        public TimeOnly From { get; init; } = TimeOnly.FromDateTime(RoundDateTime(DateTime.Now));
        public TimeOnly? To { get; init; } = TimeOnly.FromDateTime(RoundDateTime(DateTime.Now.AddHours(1)));
    }

    public sealed record AddRequest(AddTimeViewModel ViewModel) : Request<AddTimeViewModel>
    {
        public sealed record Handler(IUnitOfWorkFactory UnitOfWorkFactory) : RequestHandler<AddRequest, AddTimeViewModel>
        {
            public override async ValueTask<Response<AddTimeViewModel>> Handle(AddRequest request, CancellationToken cancellationToken)
            {
                await Task.Yield();

                var vm = request.ViewModel with
                {
                    From = RoundDateTime(request.ViewModel.From),
                    To = RoundDateTime(request.ViewModel.To),
                };

                using var uow = await UnitOfWorkFactory.CreateUnitOfWork<CTimeEntry>();

                var timeEntry = new CTimeEntry(uow)
                {
                    Description = vm.Description,
                    Date = vm.Date,
                    TimeFrom = vm.From,
                    TimeTo = vm.To
                };

                await uow.SaveAsync(timeEntry, cancellationToken);
                await uow.CommitChangesAsync(cancellationToken);

                return new AddTimeViewModel();
            }
        }
    }

    public sealed class Effect(NavigationManager navigationManager) : Effect<MediatorLoaded<AddRequest>>
    {
        public override Task HandleAsync(MediatorLoaded<AddRequest> action, IDispatcher dispatcher)
        {
            navigationManager.NavigateTo("/timetracker");
            return Task.CompletedTask;
        }
    }

    public enum RoundingDirection
    {
        Up,
        Down,
        Nearest
    }

    public static TimeOnly? RoundDateTime(TimeOnly? timeOnly, int minutes = 15, RoundingDirection direction = RoundingDirection.Nearest)
        => timeOnly.HasValue ? RoundDateTime(timeOnly.Value, minutes, direction) : null;

    public static TimeOnly RoundDateTime(TimeOnly timeOnly, int minutes = 15, RoundingDirection direction = RoundingDirection.Nearest)
        => TimeOnly.FromDateTime(RoundDateTime(new DateTime(timeOnly.Ticks), minutes, direction));

    public static DateTime RoundDateTime(DateTime dt, int minutes = 15, RoundingDirection direction = RoundingDirection.Nearest)
    {
        var t = direction switch
        {
            RoundingDirection.Up => (dt.Subtract(DateTime.MinValue)).Add(new TimeSpan(0, minutes, 0)),
            RoundingDirection.Down => (dt.Subtract(DateTime.MinValue)),
            _ => (dt.Subtract(DateTime.MinValue)).Add(new TimeSpan(0, minutes / 2, 0)),
        };

        return DateTime.MinValue.Add(new TimeSpan(0, (((int)t.TotalMinutes) / minutes) * minutes, 0));
    }
}
