using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserApi.Models;

[Table("tokens")]
public class Token
{
  [Key]
  [Column("id")]
  public Guid Id { get; set; }

  [Column("user_id")]
  public Guid UserId { get; set; }

  [Column("expired_at")]
  public DateTimeOffset ExpiredAt { get; set; }

  public User User { get; set; } = null!;
}
