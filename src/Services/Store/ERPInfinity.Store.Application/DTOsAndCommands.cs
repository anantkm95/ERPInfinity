using ERPInfinity.BuildingBlocks.CQRS;

namespace ERPInfinity.Store.Application;

public record RegisterStoreCommand(string StoreCode, string Name, string City, string Address) : ICommand<Guid>;

public record GetStoresQuery() : IQuery<List<StoreResponse>>;

public record StoreResponse(Guid StoreId, string StoreCode, string Name, string City, bool IsActive);

public class GetStoresQueryHandler : IQueryHandler<GetStoresQuery, List<StoreResponse>>
{
    public Task<Result<List<StoreResponse>>> Handle(GetStoresQuery query, CancellationToken cancellationToken = default)
    {
        var stores = new List<StoreResponse>
        {
            new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "STR-GUR-001", "ERPInfinity Gurgaon Hypermarket", "Gurgaon", true),
            new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "STR-NOI-002", "ERPInfinity Noida Supermarket", "Noida", true)
        };
        return Task.FromResult(Result<List<StoreResponse>>.Success(stores));
    }
}
