global using EcsLite;
global using EcsLite.Systems;
using Cornerstone;

MyGame myGame = new MyGame(new OpenTK.Mathematics.Vector2i(128, 128));
try
{
    myGame.Run();
}
catch (Exception e)
{
    var stream = File.Create("Crash.txt");
    using TextWriter writer = new StreamWriter(stream);
    writer.WriteLine(myGame.APIVersion.Major.ToString());
    writer.WriteLine(myGame.APIVersion.Minor.ToString());
    writer.WriteLine();
    writer.WriteLine("Vendor: " + myGame.GPUVendor);
    writer.WriteLine("OpenGL Version: " + myGame.GPUVersion);
    writer.WriteLine();
    writer.WriteLine("Exception: ");
    writer.Write(e.ToString());
}
