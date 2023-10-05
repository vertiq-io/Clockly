using System.Collections.Immutable;

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
        public ImmutableArray<TimeEntry> TimeEntries { get; init; } = ImmutableArray.Create<TimeEntry>();
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

        public bool EditMode { get; set; }
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
                    TimeEntries = items.ToImmutableArray()
                };
            }
        }
    }

    public sealed record EditModeRequest(TimeViewModel ViewModel, TimeEntry Entry) : Request<TimeViewModel>
    {
        public sealed record Handler() : RequestHandler<EditModeRequest, TimeViewModel>
        {
            public override async ValueTask<Response<TimeViewModel>> Handle(EditModeRequest request, CancellationToken cancellationToken)
            {
                await Task.CompletedTask;

                //var vm = request.ViewModel with
                //{
                //    TimeEntries = request.ViewModel.TimeEntries.Select(r => r with { EditMode = false }).ToImmutableArray()
                //};

                var vm = request.ViewModel with
                {
                    TimeEntries = request.ViewModel.TimeEntries.Replace(request.Entry, request.Entry with { EditMode = true })
                };

                return vm;
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
    public sealed record SaveRequest(TimeEntry Entry) : Request<FetchRequest>
    {
        public sealed record Handler(IUnitOfWorkFactory UnitOfWorkFactory) : RequestHandler<SaveRequest, FetchRequest>
        {
            public override async ValueTask<Response<FetchRequest>> Handle(
                SaveRequest request, 
                CancellationToken cancellationToken
            )
            {
                using var uow = await UnitOfWorkFactory.CreateUnitOfWork<CTimeEntry>();
                var item = await uow.GetObjectByKeyAsync<CTimeEntry>(request.Entry.Id, cancellationToken);
                if (item is null)
                {
                    return new FetchRequest();
                }

                item.Date = request.Entry.Date;
                item.TimeFrom = request.Entry.Start;
                item.TimeTo = request.Entry.End;
                item.Description = request.Entry.Description;

                await uow.SaveAsync(item, cancellationToken);
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

    public sealed class EffectSave : Effect<MediatorLoaded<SaveRequest>>
    {
        public override Task HandleAsync(MediatorLoaded<SaveRequest> action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new FetchRequest());
            return Task.CompletedTask;
        }
    }
}
