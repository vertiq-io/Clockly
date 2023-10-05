using Clockly.Models;

using Fluxor;

using Vertiq.Actions;
using Vertiq.DB;
using Vertiq.Messaging;

using static Clockly.Pages.TimeTracking.EnterTime;
using static MudBlazor.CategoryTypes;

namespace Clockly.Pages.TimeTracking;

public partial class TimePage
{
    public sealed record TimeViewModel
    {
        public IEnumerable<TimeEntry> TimeEntries { get; init; } = Enumerable.Empty<TimeEntry>();
    }

    public sealed record TimeEntry(
        int Id, 
        string Description, 
        DateOnly Date, 
        TimeOnly Start, 
        TimeOnly? End
    )
    {
        public decimal Duration => 
            (Convert.ToDecimal((End.HasValue ? new TimeOnly(End.Value.Ticks - Start.Ticks) : new TimeOnly())
            .ToTimeSpan()
            .TotalMinutes) / 60m);

    }

    public sealed record FetchRequest(TimeViewModel? ViewModel = null) : Request<TimeViewModel>
    {
        public FetchRequest() : this(ViewModel: null)
        {

        }

        public sealed record Handler(IUnitOfWorkFactory UnitOfWorkFactory) : RequestHandler<FetchRequest, TimeViewModel>
        {
            public override async ValueTask<Response<TimeViewModel>> Handle(FetchRequest request, CancellationToken cancellationToken)
            {
                using var uow = await UnitOfWorkFactory.CreateUnitOfWork<CTimeEntry>();

                var items = (await uow.Query<CTimeEntry>()
                    .ToListAsync(cancellationToken)
                    )
                    .Select(t => new TimeEntry(
                        t.Oid,
                        t.Description ?? "",
                        t.Date,
                        t.TimeFrom ?? TimeOnly.MinValue,
                        t.TimeTo
                    )
                ).ToList();

                return new TimeViewModel
                {
                    TimeEntries = items
                };
            }
        }
    }

    public sealed record DeleteRequest(int Id) : Request<FetchRequest>
    {
        public sealed record Handler(IUnitOfWorkFactory UnitOfWorkFactory) : RequestHandler<DeleteRequest, FetchRequest>
        {
            public override async ValueTask<Response<FetchRequest>> Handle(DeleteRequest request, CancellationToken cancellationToken)
            {
                using var uow = await UnitOfWorkFactory.CreateUnitOfWork<CTimeEntry>();
                var item = await uow.GetObjectByKeyAsync<CTimeEntry>(request.Id, cancellationToken);
                if (item is null)
                {
                    return new FetchRequest();
                }
                await uow.DeleteAsync(item, cancellationToken);
                await uow.CommitChangesAsync(cancellationToken);
                return new FetchRequest();
            }
        }
    }

    public sealed class EffectDelete : Effect<MediatorLoaded<DeleteRequest>>
    {
        public override Task HandleAsync(MediatorLoaded<DeleteRequest> action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new FetchRequest());
            return Task.CompletedTask;
        }
    }

    public sealed class EffectAdd : Effect<MediatorLoaded<AddRequest>>
    {
        public override Task HandleAsync(MediatorLoaded<AddRequest> action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new FetchRequest());
            return Task.CompletedTask;
        }
    }
}
