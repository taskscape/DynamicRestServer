using System;

namespace Dynamic.Shared.Exceptions
{
    public class EntityNotFoundException : CustomException
    {
        public int EntityId { get; }
        public Type EntityType { get; set; }

        public EntityNotFoundException(int entityId, Type entityType) : base($"Entity of type: '{entityType.Name}' with Id: '{entityId}' was not found.")
        {
            EntityId = entityId;
            EntityType = entityType;
        }
    }
}
