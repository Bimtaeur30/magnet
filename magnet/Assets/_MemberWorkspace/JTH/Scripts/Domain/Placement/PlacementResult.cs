// namespace JTH.Scripts.Domain.Placement
// {
//     public class PlacementResult
//     {
//         public bool Success { get; private set; }
//         public bool GameOver { get; private set; }
//
//         private PlacementResult(bool success, bool gameOver)
//         {
//             Success = success;
//             GameOver = gameOver;
//         }
//
//         public PlacementResult(bool gameOver)
//         {
//             Success = true;
//             GameOver = gameOver;
//         }
//
//         public static PlacementResult Failed()
//         {
//             return new PlacementResult(false, false);
//         }
//     }
// }