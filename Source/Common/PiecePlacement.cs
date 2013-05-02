using ProtoBuf;

namespace Shashkrid
{
	[ProtoContract]
	public class PiecePlacement
	{
		[ProtoMember(1)]
		public PieceTypeIdentifier Type;

		[ProtoMember(2)]
		public Position Position;

		public PiecePlacement(PieceTypeIdentifier type, Position position)
		{
			Type = type;
			Position = position;
		}
	}
}
