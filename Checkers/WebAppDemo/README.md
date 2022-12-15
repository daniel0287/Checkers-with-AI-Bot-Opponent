~~~bash
dotnet aspnet-codegenerator razorpage     -m CheckersGame     -dc ApplicationDbContext     -udl     -outDir Pages/CheckersGames     --referenceScriptLibraries    -f
dotnet aspnet-codegenerator razorpage     -m CheckersGameState     -dc ApplicationDbContext     -udl     -outDir Pages/CheckersGameStates     --referenceScriptLibraries    -f
dotnet aspnet-codegenerator razorpage     -m CheckersOption     -dc ApplicationDbContext     -udl     -outDir Pages/CheckersOptions     --referenceScriptLibraries    -f
dotnet aspnet-codegenerator razorpage     -m CheckersState     -dc ApplicationDbContext     -udl     -outDir Pages/CheckersStates     --referenceScriptLibraries    -f

dotnet aspnet-codegenerator razorpage     -m CheckersGame     -dc AppDbContext     -udl     -outDir Pages/CheckersGames     --referenceScriptLibraries    -f
dotnet aspnet-codegenerator razorpage     -m CheckersGameState     -dc AppDbContext     -udl     -outDir Pages/CheckersGameStates     --referenceScriptLibraries    -f
dotnet aspnet-codegenerator razorpage     -m CheckersOption     -dc AppDbContext     -udl     -outDir Pages/CheckersOptions     --referenceScriptLibraries    -f
dotnet aspnet-codegenerator razorpage     -m CheckersState     -dc AppDbContext     -udl     -outDir Pages/CheckersStates     --referenceScriptLibraries    -f

~~~
