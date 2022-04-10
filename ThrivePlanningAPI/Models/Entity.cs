using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThrivePlanningAPI.Models
{
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
        DateTime CreatedDate { get; set; }
        DateTime? ModifiedDate { get; set; }
        bool IsDeleted { get; set; }
    }

    public abstract class Entity<TKey> : IEntity<TKey>
    {
        protected Entity()
        {
            CreatedDate = DateTime.UtcNow;
            Id = Id is Guid ? (TKey)Convert.ChangeType(Guid.NewGuid(), typeof(TKey)) : default;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public TKey Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; } 
    }
}