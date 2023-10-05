using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;

using Clockly.Models;

using Vertiq.DB;
using Vertiq.Messaging;

namespace Clockly.Pages.TimeTracking;

public partial class TimePage
{
    public sealed record TimeViewModel
    {
        public IEnumerable<TimeEntry> TimeEntries { get; init; } = Enumerable.Empty<TimeEntry>();
    }

    public sealed record TimeEntry(string Description, DateOnly Date, TimeOnly? Start, TimeOnly? End)
    {

    }

    public sealed record FetchRequest(TimeViewModel? ViewModel = null) : Request<TimeViewModel>
    {
        public sealed record Handler(IUnitOfWorkFactory UnitOfWorkFactory) : RequestHandler<FetchRequest, TimeViewModel>
        {
            public override async ValueTask<Response<TimeViewModel>> Handle(FetchRequest request, CancellationToken cancellationToken)
            {
                using var uow = await UnitOfWorkFactory.CreateUnitOfWork<CTimeEntry>();

                var items = await uow.Query<CTimeEntry>()
                    .Select(t => new TimeEntry(
                        t.Description ?? "", 
                        t.Date, 
                        t.TimeFrom, 
                        t.TimeTo
                    )
                ).ToListAsync(cancellationToken);

                return new TimeViewModel
                {
                    TimeEntries = items
                };
            }
        }
    }
}
