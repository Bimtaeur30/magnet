namespace JTH.Scripts.Domain.Rotation
{
    public sealed class BoardRotationService
    {
        public const int DefaultDegreesClockwise = 90;

        public void RotateClockwise(BoardSession session)
        {
            session.RotateAllClockwise90();
        }
    }
}
