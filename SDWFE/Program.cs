using System;

try

{
    using (var game = new SDWFE.SDWFEGame())
        game.Run();
}

catch (Exception e)
{
    System.IO.File.WriteAllText("crash.log", e.Message + Environment.NewLine + e.StackTrace);
}  