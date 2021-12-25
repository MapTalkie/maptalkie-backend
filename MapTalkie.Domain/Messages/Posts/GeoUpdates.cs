using System.Collections.Generic;
using MapTalkie.Domain.Utils;
using Newtonsoft.Json;

namespace MapTalkie.Domain.Messages.Posts;

public record GeoUpdates
{
    [JsonConstructor]
    public GeoUpdates(IList<GeoUpdate> updates)
    {
        Updates = updates;
    }

    public IList<GeoUpdate> Updates { get; }
}

public record GeoUpdate
{
    [JsonConstructor]
    public GeoUpdate(
        AreaId id,
        IList<PostCreated> newPosts)
    {
        Id = id;
        NewPosts = newPosts;
    }

    public AreaId Id { get; }
    public IList<PostCreated> NewPosts { get; }
    public IList<PostDeleted> DeletedPosts { get; set; }
}