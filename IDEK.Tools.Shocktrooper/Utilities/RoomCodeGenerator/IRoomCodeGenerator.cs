namespace IDEK.Tools.ShocktroopUtils
{
    public interface IRoomCodeGenerator
    {
        string HashIntoRoomCode(string userGUID);
        string NumIntoDisplayableRoomCode(long num);
        string OffsetRoomCode(string roomCode, int numericalOffset);
        long RoomCodeToNumber(string roomCode);
        bool IsValidRoomCode(string roomCode);
        char[] ValidRoomCodeChars { get; }
        bool ShouldForceUppercase();
    }
}
