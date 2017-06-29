namespace AJHoloServer
{
  using com.mtaulty.AJHoloServer;
  using System;

  class HologramDetails
  {
    public HologramDetails(
      Guid anchorId,
      Guid hologramId,
      string hologramTypeName,
      AJHoloServerPosition position)
    {
      this.AnchorId = anchorId;
      this.HologramId = hologramId;
      this.HologramTypeName = hologramTypeName;
      this.Position = position;
    }
    public Guid AnchorId { get; private set; }
    public Guid HologramId { get; private set; }
    public string HologramTypeName { get; private set; }
    public AJHoloServerPosition Position { get; set; } 
  }
}
