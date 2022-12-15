using System.Text.Json;
using Domain;

namespace GameBrain;

public class CheckersBrain
{
    private readonly CheckersState _state;

    public CheckersBrain(CheckersOption option, CheckersGameState? state)
    {
        if (state == null)
        {
            _state = new CheckersState();
            InitializeNewGame(option);
        }
        else
        {
            _state = JsonSerializer.Deserialize<CheckersState>(state.SerializedGameState)!;
        }
    }

    public string GetSerializedGameState()
    {
        return JsonSerializer.Serialize(_state);
    }

    private void InitializeNewGame(CheckersOption option)
    {
        var boardWidth = option.Width;
        var boardHeight = option.Height;

        if (boardWidth < 4 || boardHeight < 4)
        {
            throw new ArgumentException("Board size too small");
        }

        if (boardWidth % 2 != 0)
        {
            throw new ArgumentException("Width is not an even number");
        }

        if (boardHeight % 2 != 0)
        {
            throw new ArgumentException("Height is not an even number");
        }

        // Initialize the jagged array
        _state.GameBoard = new EGameTileState?[boardWidth][];
        for (int i = 0; i < boardWidth; i++)
        {
            _state.GameBoard[i] = new EGameTileState?[boardHeight];
        }

        var blackInitialX = 1;
        var blackInitialY = 0;
        var blackSwitched = false;

        for (var i = 0; i < (boardHeight / 2 - 1) * (boardWidth / 2); i++)
        {
            _state.GameBoard[blackInitialX][blackInitialY] = EGameTileState.BlackStandard;
            if (blackInitialX < boardWidth - 2)
            {
                blackInitialX += 2;
            }
            else
            {
                blackInitialY += 1;
                if (!blackSwitched)
                {
                    blackInitialX = 0;
                    blackSwitched = true;
                }
                else
                {
                    blackInitialX = 1;
                    blackSwitched = false;
                }
            }
        }

        var whiteInitialX = 0;
        var whiteInitialY = boardHeight - 1;
        var whiteSwitched = false;

        for (int i = 0; i < (boardHeight / 2 - 1) * (boardWidth / 2); i++)
        {
            _state.GameBoard[whiteInitialX][whiteInitialY] = EGameTileState.RedStandard;
            if (whiteInitialX < boardWidth - 2)
            {
                whiteInitialX += 2;
            }
            else
            {
                whiteInitialY -= 1;
                if (!whiteSwitched)
                {
                    whiteInitialX = 1;
                    whiteSwitched = true;
                }
                else
                {
                    whiteInitialX = 0;
                    whiteSwitched = false;
                }
            }
        }
    }

    public EGameTileState?[][] GetBoard()
    {
        var jsonStr = JsonSerializer.Serialize(_state.GameBoard);
        return JsonSerializer.Deserialize<EGameTileState?[][]>(jsonStr)!;
    }

    private int BoardWidth() => GetBoard().Length;

    private int BoardHeight() => GetBoard()[0].Length;

    public bool NextMoveByBlack() => _state.NextMoveByBlack;

    private bool RedPieceOnTile(int x, int y)
    {
        return _state.GameBoard[x][y] == EGameTileState.RedStandard || _state.GameBoard[x][y] == EGameTileState.RedKing;
    }
    private bool BlackPieceOnTile(int x, int y)
    {
        return _state.GameBoard[x][y] == EGameTileState.BlackStandard || _state.GameBoard[x][y] == EGameTileState.BlackKing;
    }

    private bool RedKingOnTile(int x, int y)
    {
        return _state.GameBoard[x][y] == EGameTileState.SelectedRedKing ||
               _state.GameBoard[x][y] == EGameTileState.RedKing;
    }
    
    private bool BlackKingOnTile(int x, int y)
    {
        return _state.GameBoard[x][y] == EGameTileState.SelectedBlackKing ||
               _state.GameBoard[x][y] == EGameTileState.BlackKing;
    }

    private bool SelectedRedPieceOnTile(int x, int y)
    {
        return _state.GameBoard[x][y] == EGameTileState.SelectedRedStandard ||
               _state.GameBoard[x][y] == EGameTileState.SelectedRedKing;
    }
    
    private bool SelectedBlackPieceOnTile(int x, int y)
    {
        return _state.GameBoard[x][y] == EGameTileState.SelectedBlackStandard ||
               _state.GameBoard[x][y] == EGameTileState.SelectedBlackKing;
    }
    
    private bool TileIsPossibleOrForced(int x, int y)
    {
        return _state.GameBoard[x][y] == EGameTileState.PossibleMove || _state.GameBoard[x][y] == EGameTileState.ForcedMove;
    }

    public void MakeAMove(int x, int y)
    {
        if (TileIsPossibleOrForced(x, y))
        {
            Move(x, y);
        }
        else if (ExistenceOfForcedJumps())
        {
        }
        else if (JumpsAvailableWithoutForced())
        {
            if (CanEat(x, y) && (NextMoveByBlack() && BlackPieceOnTile(x, y) || !NextMoveByBlack() && RedPieceOnTile(x, y)))
            {
                ClearRedundant();
                ShowPossibleMoves(x, y);
            }
            else
            {
                ClearRedundant();
            }
        }
        else if (NextMoveByBlack() && BlackPieceOnTile(x, y) || !NextMoveByBlack() && RedPieceOnTile(x, y))
        {
            ClearRedundant();
            ShowPossibleMoves(x, y);
        }
        else
        {
            ClearRedundant();
        }
    }

    public void MakeAMoveByAi()
    {
        var tempBoard = JsonSerializer.Deserialize<EGameTileState?[][]>(JsonSerializer.Serialize(GetBoard()));
        const int depth = 3;
        var newBoard = MiniMax(tempBoard!, depth, NextMoveByBlack()).Item2;

        _state.GameBoard = JsonSerializer.Deserialize<EGameTileState?[][]>(JsonSerializer.Serialize(newBoard))!;
        
        if (ExistenceOfForcedJumps())
        {
            MakeAMoveByAi();
        }
        else
        {
            _state.NextMoveByBlack = !_state.NextMoveByBlack;
        }
    }

    private void ClearRedundant()
    {
        for (var i = 0; i < BoardWidth(); i++)
        {
            for (var j = 0; j < BoardHeight(); j++)
            {
                _state.GameBoard[i][j] = _state.GameBoard[i][j] switch
                {
                    EGameTileState.PossibleMove => null,
                    EGameTileState.SelectedBlackStandard => EGameTileState.BlackStandard,
                    EGameTileState.SelectedRedStandard => EGameTileState.RedStandard,
                    EGameTileState.SelectedBlackKing => EGameTileState.BlackKing,
                    EGameTileState.SelectedRedKing => EGameTileState.RedKing,
                    _ => _state.GameBoard[i][j]
                };
            }
        }
    }
    
    private void Move(int x, int y) 
    {
        var turnStays = false;
        var selectedTypeStandard = true;
        
        for (var i = 0; i < BoardWidth(); i++)
        {
            for (var j = 0; j < BoardHeight(); j++)
            {
                if (TileIsPossibleOrForced(i, j))
                {
                    _state.GameBoard[i][j] = null;
                }

                switch (_state.GameBoard[i][j])
                {
                    case EGameTileState.SelectedBlackStandard:
                    {
                        _state.GameBoard[x][y] = EGameTileState.BlackStandard;
                        if (_state.GameBoard[(x + i) / 2][y - 1] == EGameTileState.RedStandard || _state.GameBoard[(x + i) / 2][y - 1] == EGameTileState.RedKing)
                        {
                            _state.GameBoard[(x + i) / 2][y - 1] = null;
                            if (CanEat(x, y))
                            {
                                turnStays = true;
                            }
                        }
                        _state.GameBoard[i][j] = null;
                        break;
                    }
                    case EGameTileState.SelectedRedStandard:
                    {
                        _state.GameBoard[x][y] = EGameTileState.RedStandard;
                        if (_state.GameBoard[(x + i) / 2][y + 1] == EGameTileState.BlackStandard || _state.GameBoard[(x + i) / 2][y + 1] == EGameTileState.BlackKing)
                        {
                            _state.GameBoard[(x + i) / 2][y + 1] = null;
                            if (CanEat(x, y))
                            {
                                turnStays = true;
                            }
                        }
                        _state.GameBoard[i][j] = null;
                        break;
                    }
                    case EGameTileState.SelectedBlackKing:
                    {
                        selectedTypeStandard = false;
                        _state.GameBoard[x][y] = EGameTileState.BlackKing;

                        // Checks if BlackKing moved forward or backwards. If j > y, then he moved forward, otherwise backwards.
                        if (j > y)
                        {
                            if (_state.GameBoard[(x + i) / 2][y + 1] == EGameTileState.RedStandard || _state.GameBoard[(x + i) / 2][y + 1] == EGameTileState.RedKing)
                            {
                                _state.GameBoard[(x + i) / 2][y + 1] = null;
                                if (CanEat(x, y))
                                {
                                    turnStays = true;
                                }
                            }
                        }
                        else
                        {
                            if (_state.GameBoard[(x + i) / 2][y - 1] == EGameTileState.RedStandard || _state.GameBoard[(x + i) / 2][y - 1] == EGameTileState.RedKing)
                            {
                                _state.GameBoard[(x + i) / 2][y - 1] = null;
                                if (CanEat(x, y))
                                {
                                    turnStays = true;
                                }
                            }
                        }
                    
                        _state.GameBoard[i][j] = null;
                        break;
                    }
                    case EGameTileState.SelectedRedKing:
                    {
                        selectedTypeStandard = false;
                        _state.GameBoard[x][y] = EGameTileState.RedKing;
                        
                        // Checks if RedKing moved forward or backwards. If j > y, then he moved backwards, otherwise forward.
                        if (j > y)
                        {
                            if (_state.GameBoard[(x + i) / 2][y + 1] == EGameTileState.BlackStandard || _state.GameBoard[(x + i) / 2][y + 1] == EGameTileState.BlackKing)
                            {
                                _state.GameBoard[(x + i) / 2][y + 1] = null;
                                if (CanEat(x, y))
                                {
                                    turnStays = true;
                                }
                            }
                        }
                    
                        else
                        {
                            if (_state.GameBoard[(x + i) / 2][y - 1] == EGameTileState.BlackStandard || _state.GameBoard[(x + i) / 2][y - 1] == EGameTileState.BlackKing)
                            {
                                _state.GameBoard[(x + i) / 2][y - 1] = null;
                                if (CanEat(x, y))
                                {
                                    turnStays = true;
                                }
                            }
                        }
                    
                        _state.GameBoard[i][j] = null;
                        break;
                    }
                }
            }
        }
        
        if (y == 0 && !_state.NextMoveByBlack)
        {
            _state.GameBoard[x][y] = EGameTileState.RedKing;
        }
        
        else if (y == BoardHeight() - 1 && _state.NextMoveByBlack)
        {
            _state.GameBoard[x][y] = EGameTileState.BlackKing;
        }

        if (!turnStays)
        {
            _state.NextMoveByBlack = !_state.NextMoveByBlack;
        }
        else
        {
            if (selectedTypeStandard)
            {
                _state.GameBoard[x][y] = _state.NextMoveByBlack ? EGameTileState.SelectedBlackStandard : EGameTileState.SelectedRedStandard;

            }
            else
            {
                _state.GameBoard[x][y] = _state.NextMoveByBlack ? EGameTileState.SelectedBlackKing : EGameTileState.SelectedRedKing;
            }
            ShowForcedMoves(x, y);
        }
    }
    
    public bool PieceHasMoves(int x, int y)
    {
        if (RedPieceOnTile(x, y) && !NextMoveByBlack())
        {
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                if (_state.GameBoard[x - 1][y - 1] == null)
                {
                    return true;
                }
            }

            if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
            {
                if (_state.GameBoard[x + 1][y - 1] == null)
                {
                    return true;
                }
            }

            if (RedKingOnTile(x, y))
            {
                if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                {
                    if (_state.GameBoard[x - 1][y + 1] == null)
                    {
                        return true;
                    }
                }

                if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                {
                    if (_state.GameBoard[x + 1][y + 1] == null)
                    {
                        return true;
                    }
                }
            }
        }
        else if (BlackPieceOnTile(x, y) && NextMoveByBlack())
        {
            if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
            {
                if (_state.GameBoard[x - 1][y + 1] == null)
                {
                    return true;
                }
            }

            if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
            {
                if (_state.GameBoard[x + 1][y + 1] == null)
                {
                    return true;
                }
            }

            if (BlackKingOnTile(x, y))
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    if (_state.GameBoard[x - 1][y - 1] == null)
                    {
                        return true;
                    }
                }

                if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                {
                    if (_state.GameBoard[x + 1][y - 1] == null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void ShowPossibleMoves(int x, int y)
    {
        if (RedPieceOnTile(x, y))
        {
            if (_state.GameBoard[x][y] == EGameTileState.RedStandard)
            {
                _state.GameBoard[x][y] = EGameTileState.SelectedRedStandard;
            }
            else
            {
                _state.GameBoard[x][y] = EGameTileState.SelectedRedKing;
            }
            
            if (!GiveEatingOptionsIfCanEatForRed(x, y))
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    _state.GameBoard[x - 1][y - 1] ??= EGameTileState.PossibleMove;
                }

                if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                {
                    _state.GameBoard[x + 1][y - 1] ??= EGameTileState.PossibleMove;
                }

                if (_state.GameBoard[x][y] == EGameTileState.SelectedRedKing)
                {
                    if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                    {
                        _state.GameBoard[x - 1][y + 1] ??= EGameTileState.PossibleMove;
                    }

                    if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                    {
                        _state.GameBoard[x + 1][y + 1] ??= EGameTileState.PossibleMove;
                    }
                }
            }
        }
        
        else if (BlackPieceOnTile(x, y))
        {
            if (_state.GameBoard[x][y] == EGameTileState.BlackStandard)
            {
                _state.GameBoard[x][y] = EGameTileState.SelectedBlackStandard;
            }
            else
            {
                _state.GameBoard[x][y] = EGameTileState.SelectedBlackKing;
            }
            
            if (!GiveEatingOptionsIfCanEatForBlack(x, y))
            {
                if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                {
                    _state.GameBoard[x - 1][y + 1] ??= EGameTileState.PossibleMove;
                }

                if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                {
                    _state.GameBoard[x + 1][y + 1] ??= EGameTileState.PossibleMove;
                }

                if (_state.GameBoard[x][y] == EGameTileState.SelectedBlackKing)
                {
                    if (x - 1 >= 0 && y - 1 >= 0)
                    {
                        _state.GameBoard[x - 1][y - 1] ??= EGameTileState.PossibleMove;
                    }

                    if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                    {
                        _state.GameBoard[x + 1][y - 1] ??= EGameTileState.PossibleMove;
                    }
                }
            }
        }
    }

    private void ShowForcedMoves(int x, int y)
    {
        if (SelectedRedPieceOnTile(x, y))
        {
            if (!GiveForcedEatingOptionsForRed(x, y))
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    _state.GameBoard[x - 1][y - 1] ??= EGameTileState.ForcedMove;
                }

                if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                {
                    _state.GameBoard[x + 1][y - 1] ??= EGameTileState.ForcedMove;
                }

                if (_state.GameBoard[x][y] == EGameTileState.SelectedRedKing)
                {
                    if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                    {
                        _state.GameBoard[x - 1][y + 1] ??= EGameTileState.ForcedMove;
                    }

                    if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                    {
                        _state.GameBoard[x + 1][y + 1] ??= EGameTileState.ForcedMove;
                    }
                }
            }
        }

        else
        {
            if (!GiveForcedEatingOptionsForBlack(x, y))
            {
                if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                {
                    _state.GameBoard[x - 1][y + 1] ??= EGameTileState.ForcedMove;
                }

                if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                {
                    _state.GameBoard[x + 1][y + 1] ??= EGameTileState.ForcedMove;
                }

                if (_state.GameBoard[x][y] == EGameTileState.SelectedBlackKing)
                {
                    if (x - 1 >= 0 && y - 1 >= 0)
                    {
                        _state.GameBoard[x - 1][y - 1] ??= EGameTileState.ForcedMove;
                    }

                    if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                    {
                        _state.GameBoard[x + 1][y - 1] ??= EGameTileState.ForcedMove;
                    }
                }
            }
        }
    }

    private bool GiveEatingOptionsIfCanEatForRed(int x, int y)
    {
        var canEat = false;
        
        if (x - 1 >= 0 && y - 1 >= 0)
        {
            if ((BlackPieceOnTile(x - 1, y - 1)) && x - 2 >= 0 && y - 2 >= 0)
            {
                if (_state.GameBoard[x - 2][y - 2] == null)
                {
                    _state.GameBoard[x - 2][y - 2] = EGameTileState.PossibleMove;
                    canEat = true;
                }
            }
        }

        if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
        {
            if ((BlackPieceOnTile(x + 1, y - 1)) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
            {
                if (_state.GameBoard[x + 2][y - 2] == null)
                {
                    _state.GameBoard[x + 2][y - 2] = EGameTileState.PossibleMove;
                    canEat = true;
                }
            }
        }

        if (_state.GameBoard[x][y] == EGameTileState.SelectedRedKing)
        {
            if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
            {
                if ((BlackPieceOnTile(x - 1, y + 1)) && x - 2 >= 0 &&
                    y + 2 <= BoardHeight() - 1)
                { 
                    if (_state.GameBoard[x - 2][y + 2] == null)
                    {
                        _state.GameBoard[x - 2][y + 2] = EGameTileState.PossibleMove;
                        canEat = true;
                    }
                }
            }

            if (x + 1 <= BoardWidth() - 1 && y + 1 <= BoardHeight() - 1)
            {
                if ((BlackPieceOnTile(x + 1, y + 1)) && x + 2 <= BoardWidth() - 1 &&
                    y + 2 <= BoardHeight() - 1)
                {
                    if (_state.GameBoard[x + 2][y + 2] == null)
                    { 
                        _state.GameBoard[x + 2][y + 2] = EGameTileState.PossibleMove;
                        canEat = true;
                    }
                }
            }
        }

        return canEat;
    }

    private bool GiveEatingOptionsIfCanEatForBlack(int x, int y)
    {
        var canEat = false;

        if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
        {
            if ((RedPieceOnTile(x - 1, y + 1)) && x - 2 >= 0 &&
                y + 2 <= BoardHeight() - 1)
            { 
                if (_state.GameBoard[x - 2][y + 2] == null)
                {
                    _state.GameBoard[x - 2][y + 2] = EGameTileState.PossibleMove;
                    canEat = true;
                }
            }
        }

        if (x + 1 <= BoardWidth() - 1 && y + 1 <= BoardHeight() - 1)
        {
            if ((RedPieceOnTile(x + 1, y + 1)) && x + 2 <= BoardWidth() - 1 &&
                y + 2 <= BoardHeight() - 1)
            {
                if (_state.GameBoard[x + 2][y + 2] == null)
                { 
                    _state.GameBoard[x + 2][y + 2] = EGameTileState.PossibleMove;
                    canEat = true;
                }
            }
        }

        if (_state.GameBoard[x][y] == EGameTileState.SelectedBlackKing)
        {
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                if ((RedPieceOnTile(x - 1, y - 1)) && x - 2 >= 0 && y - 2 >= 0)
                {
                    if (_state.GameBoard[x - 2][y - 2] == null)
                    {
                        _state.GameBoard[x - 2][y - 2] = EGameTileState.PossibleMove;
                        canEat = true;
                    }
                }
            }

            if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
            {
                if ((RedPieceOnTile(x + 1, y - 1)) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
                {
                    if (_state.GameBoard[x + 2][y - 2] == null)
                    {
                        _state.GameBoard[x + 2][y - 2] = EGameTileState.PossibleMove;
                        canEat = true;
                    }
                }
            }
        }
        
        return canEat;
    }
    
    private bool GiveForcedEatingOptionsForRed(int x, int y)
    {
        var canEat = false;
        
        if (x - 1 >= 0 && y - 1 >= 0)
        {
            if (BlackPieceOnTile(x - 1, y - 1) && x - 2 >= 0 && y - 2 >= 0)
            {
                if (_state.GameBoard[x - 2][y - 2] == null)
                {
                    _state.GameBoard[x - 2][y - 2] = EGameTileState.ForcedMove;
                    canEat = true;
                }
            }
        }

        if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
        {
            if (BlackPieceOnTile(x + 1, y - 1) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
            {
                if (_state.GameBoard[x + 2][y - 2] == null)
                {
                    _state.GameBoard[x + 2][y - 2] = EGameTileState.ForcedMove;
                    canEat = true;
                }
            }
        }

        if (_state.GameBoard[x][y] == EGameTileState.SelectedRedKing)
        {
            if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
            {
                if (BlackPieceOnTile(x - 1, y + 1) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
                { 
                    if (_state.GameBoard[x - 2][y + 2] == null)
                    {
                        _state.GameBoard[x - 2][y + 2] = EGameTileState.ForcedMove;
                        canEat = true;
                    }
                }
            }

            if (x + 1 <= BoardWidth() - 1 && y + 1 <= BoardHeight() - 1)
            {
                if (BlackPieceOnTile(x + 1, y + 1) && x + 2 <= BoardWidth() - 1 && y + 2 <= BoardHeight() - 1)
                {
                    if (_state.GameBoard[x + 2][y + 2] == null)
                    { 
                        _state.GameBoard[x + 2][y + 2] = EGameTileState.ForcedMove;
                        canEat = true;
                    }
                }
            }
        }

        return canEat;
    }

    private bool GiveForcedEatingOptionsForBlack(int x, int y)
    {
        var canEat = false;

        if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
        {
            if (RedPieceOnTile(x - 1, y + 1) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
            { 
                if (_state.GameBoard[x - 2][y + 2] == null)
                {
                    _state.GameBoard[x - 2][y + 2] = EGameTileState.ForcedMove;
                    canEat = true;
                }
            }
        }

        if (x + 1 <= BoardWidth() - 1 && y + 1 <= BoardHeight() - 1)
        {
            if (RedPieceOnTile(x + 1, y + 1) && x + 2 <= BoardWidth() - 1 && y + 2 <= BoardHeight() - 1)
            {
                if (_state.GameBoard[x + 2][y + 2] == null)
                { 
                    _state.GameBoard[x + 2][y + 2] = EGameTileState.ForcedMove;
                    canEat = true;
                }
            }
        }

        if (_state.GameBoard[x][y] == EGameTileState.SelectedBlackKing)
        {
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                if (RedPieceOnTile(x - 1, y - 1) && x - 2 >= 0 && y - 2 >= 0)
                {
                    if (_state.GameBoard[x - 2][y - 2] == null)
                    {
                        _state.GameBoard[x - 2][y - 2] = EGameTileState.ForcedMove;
                        canEat = true;
                    }
                }
            }

            if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
            {
                if (RedPieceOnTile(x + 1, y - 1) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
                {
                    if (_state.GameBoard[x + 2][y - 2] == null)
                    {
                        _state.GameBoard[x + 2][y - 2] = EGameTileState.ForcedMove;
                        canEat = true;
                    }
                }
            }
        }
        
        return canEat;
    }

    private bool CanEat(int x, int y)
    {
        if (NextMoveByBlack())
        {
            if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
            {
                if (RedPieceOnTile(x - 1, y + 1) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
                {
                    if (_state.GameBoard[x - 2][y + 2] == null || _state.GameBoard[x - 2][y + 2] == EGameTileState.PossibleMove)
                    {
                        return true;
                    }
                }
            }

            if (BoardWidth() - 1 >= x + 1 && y + 1 <= BoardHeight() - 1)
            {
                if (RedPieceOnTile(x + 1, y + 1) && x + 2 <= BoardWidth() - 1 && y + 2 <= BoardHeight() - 1)
                {
                    if (_state.GameBoard[x + 2][y + 2] == null || _state.GameBoard[x + 2][y + 2] == EGameTileState.PossibleMove)
                    {
                        return true;
                    }
                }
            }

            if (BlackKingOnTile(x, y))
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    if (RedPieceOnTile(x - 1, y - 1) && x - 2 >= 0 && y - 2 >= 0)
                    {
                        if (_state.GameBoard[x - 2][y - 2] == null || _state.GameBoard[x - 2][y - 2] == EGameTileState.PossibleMove)
                        {
                            return true;
                        }
                    }
                }
                if (x + 1 <= BoardWidth() - 1 && y - 1 >= 0)
                {
                    if (RedPieceOnTile(x + 1, y - 1) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
                    {
                        if (_state.GameBoard[x + 2][y - 2] == null || _state.GameBoard[x + 2][y - 2] == EGameTileState.PossibleMove)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        else
        {
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                if (BlackPieceOnTile(x - 1, y - 1) && x - 2 >= 0 && y - 2 >= 0)
                {
                    if (_state.GameBoard[x - 2][y - 2] == null || _state.GameBoard[x - 2][y - 2] == EGameTileState.PossibleMove)
                    {
                        return true;
                    }
                }
            }
            if (x + 1 <= BoardWidth() - 1 && y - 1 >= 0)
            {
                if (BlackPieceOnTile(x + 1, y - 1) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
                {
                    if (_state.GameBoard[x + 2][y - 2] == null || _state.GameBoard[x + 2][y - 2] == EGameTileState.PossibleMove)
                    {
                        return true;
                    }
                }
            }

            if (RedKingOnTile(x, y))
            {
                if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
                {
                    if (BlackPieceOnTile(x - 1, y + 1) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
                    {
                        if (_state.GameBoard[x - 2][y + 2] == null || _state.GameBoard[x - 2][y + 2] == EGameTileState.PossibleMove)
                        {
                            return true;
                        }
                    }
                }

                if (BoardWidth() - 1 >= x + 1 && y + 1 <= BoardHeight() - 1)
                {
                    if (BlackPieceOnTile(x + 1, y + 1) && x + 2 <= BoardWidth() - 1 && y + 2 <= BoardHeight() - 1)
                    {
                        if (_state.GameBoard[x + 2][y + 2] == null || _state.GameBoard[x + 2][y + 2] == EGameTileState.PossibleMove)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }

    public bool JumpsAvailableWithoutForced()
    {
        if (NextMoveByBlack())
        {
            for (int i = 0; i < BoardWidth(); i++)
            {
                for (int j = 0; j < BoardHeight(); j++)
                {
                    if (BlackPieceOnTile(i, j) || SelectedBlackPieceOnTile(i, j))
                    {
                        if (CanEat(i, j))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < BoardWidth(); i++)
            {
                for (int j = 0; j < BoardHeight(); j++)
                {
                    if (RedPieceOnTile(i, j) || SelectedRedPieceOnTile(i, j))
                    {
                        if (CanEat(i, j))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool ExistenceOfForcedJumps()
    {
        for (var i = 0; i < BoardWidth(); i++)
        {
            for (var j = 0; j < BoardHeight(); j++)
            {
                if (_state.GameBoard[i][j] == EGameTileState.ForcedMove)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    private bool RedPieceOnTileInCopy(int x, int y, EGameTileState?[][] board)
    {
        return board[x][y] == EGameTileState.RedStandard || board[x][y] == EGameTileState.RedKing;
    }
    
    private bool BlackPieceOnTileInCopy(int x, int y, EGameTileState?[][] board)
    {
        return board[x][y] == EGameTileState.BlackStandard || board[x][y] == EGameTileState.BlackKing;
    }

    private bool RedKingOnTileInCopy(int x, int y, EGameTileState?[][] board)
    {
        return board[x][y] == EGameTileState.SelectedRedKing || board[x][y] == EGameTileState.RedKing;
    }
    
    private bool BlackKingOnTileInCopy(int x, int y, EGameTileState?[][] board)
    {
        return board[x][y] == EGameTileState.SelectedBlackKing || board[x][y] == EGameTileState.BlackKing;
    }

    private bool SelectedRedPieceOnTileInCopy(int x, int y, EGameTileState?[][] board)
    {
        return board[x][y] == EGameTileState.SelectedRedStandard || board[x][y] == EGameTileState.SelectedRedKing;
    }
    
    private bool SelectedBlackPieceOnTileInCopy(int x, int y, EGameTileState?[][] board)
    {
        return board[x][y] == EGameTileState.SelectedBlackStandard || board[x][y] == EGameTileState.SelectedBlackKing;
    }
    
    private bool TileIsPossibleOrForcedInCopy(int x, int y, EGameTileState?[][] board)
    {
        return board[x][y] == EGameTileState.PossibleMove || board[x][y] == EGameTileState.ForcedMove;
    }
    
    private (int, EGameTileState?[][]) MiniMax(EGameTileState?[][] board, int depth, bool maxPlayer)
    {
        if (depth == 0 || CheckIfGameOver(board, maxPlayer))
        {
            return (Utility(board), board);
        }
        
        if (maxPlayer)
        {
            var maxEval = int.MinValue;
            EGameTileState?[][] bestMove = default!;
            foreach (var move in GetAllMoves(board, true))
            {
                var evaluation = MiniMax(move, depth - 1, false).Item1;
                if (maxEval == evaluation)
                {
                    var previousMove = EvaluateDistance(bestMove);
                    var currentMove = EvaluateDistance(move);
                    var maxDist = Math.Max(previousMove, currentMove);
                    if (maxDist == currentMove)
                    {
                        bestMove = move;
                    }
                }
                else
                {
                    maxEval = Math.Max(maxEval, evaluation);
                    if (maxEval == evaluation)
                    {
                        bestMove = move;
                    } 
                }
                
            }
            return (maxEval, bestMove);
        }
        else
        {
            int minEval = int.MaxValue;
            EGameTileState?[][] bestMove = default!;
            foreach (var move in GetAllMoves(board, false))
            {
                int evaluation = MiniMax(move, depth - 1, true).Item1;
                if (minEval == evaluation)
                {
                    var previousMove = EvaluateDistance(bestMove);
                    var currentMove = EvaluateDistance(move);
                    int minDist = Math.Min(previousMove, currentMove);
                    if (minDist == currentMove)
                    {
                        bestMove = move;
                    }
                }
                else
                {
                    minEval = Math.Min(minEval, evaluation);
                    if (minEval == evaluation)
                    {
                        bestMove = move;
                    }
                }
            }

            return (minEval, bestMove);
        }
    }

    private List<(int, int)> GetAllMovablePieces(EGameTileState?[][] board, bool currentPlayerBlack)
    {
        var movablePieces = new List<(int x, int y)>();

        if (!ExistenceOfForcedJumpsInCopy(board))
        {
            for (var x = 0; x < BoardWidth(); x++)
            {
                var diff = x % 2 == 0 ? 1 : 0;

                for (var y = diff; y < BoardHeight(); y += 2)
                {
                    if (JumpsAvailableWithoutForcedInCopy(board, currentPlayerBlack))
                    {
                        if (CanEatInCopy(x, y, board, currentPlayerBlack) && (currentPlayerBlack
                                && BlackPieceOnTileInCopy(x, y, board)
                                || !currentPlayerBlack && RedPieceOnTileInCopy(x, y, board)))
                        {
                            movablePieces.Add((x, y));
                        }
                    }
                    else
                    {
                        if (PieceHasMovesInCopy(x, y, board, currentPlayerBlack))
                        {
                            movablePieces.Add((x, y));
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = 0; x < BoardWidth(); x++)
            {
                int diff = x % 2 == 0 ? 1 : 0;

                for (int y = diff; y < BoardHeight(); y += 2)
                {
                    if (board[x][y] == EGameTileState.ForcedMove)
                    {
                        movablePieces.Add((x, y));
                    }
                }
            }
        }
        return movablePieces;
    }
    
    private List<EGameTileState?[][]> GetAllMoves(EGameTileState?[][] board, bool currentPlayerBlack)
    {
        List<EGameTileState?[][]> moves = new List<EGameTileState?[][]>();
        foreach (var (x, y) in GetAllMovablePieces(board, currentPlayerBlack))
        {
            var copyOfBoard = JsonSerializer.Deserialize<EGameTileState?[][]>(JsonSerializer.Serialize(board));
            EGameTileState?[][] tempBoard = ShowPossibleMovesInCopy(x, y, copyOfBoard!);

            for (int i = 0; i < BoardWidth(); i++)
            {
                for (int j = 0; j < BoardHeight(); j++)
                {
                    if (TileIsPossibleOrForcedInCopy(i, j, tempBoard))
                    {
                        var copyOfBoard2 = JsonSerializer.Deserialize<EGameTileState?[][]>(JsonSerializer.Serialize(tempBoard));
                        EGameTileState?[][] newBoard = MoveInCopy(i, j, copyOfBoard2!, currentPlayerBlack);
                        moves.Add(newBoard);
                    }
                }
            }
        }

        return moves;
    }

    private int Utility(EGameTileState?[][] board) 
    {
        var redStandardPieces = 0;
        var redKingPieces = 0;
        var blackStandardPieces = 0;
        var blackKingPieces = 0;
        for (var x = 0; x < BoardWidth(); x++)
        {
            for (var y = 0; y < BoardHeight(); y++)
            {
                switch (board[x][y])
                {
                    case EGameTileState.RedStandard:
                        redStandardPieces++;
                        break;
                    case EGameTileState.RedKing:
                        redKingPieces++;
                        break;
                    case EGameTileState.BlackStandard:
                        blackStandardPieces++;
                        break;
                    case EGameTileState.BlackKing:
                        blackKingPieces++;
                        break;
                }
            }
        }

        return blackStandardPieces * 2 - redStandardPieces * 2 + (blackKingPieces * 3 - redKingPieces * 3);
    }

    private int EvaluateDistance(EGameTileState?[][] board)
    {
        var redPiecesDistance = 0;
        var blackPiecesDistance = 0;
        for (var x = 0; x < BoardWidth(); x++)
        {
            for (var y = 0; y < BoardHeight(); y++)
            {
                switch (board[x][y])
                {
                    case EGameTileState.RedStandard:
                        redPiecesDistance += BoardWidth() - y;
                        break;
                    case EGameTileState.BlackStandard:
                        blackPiecesDistance += y + 1;
                        break;
                    case EGameTileState.RedKing:
                        redPiecesDistance += BoardWidth() + 1;
                        break;
                    case EGameTileState.BlackKing:
                        blackPiecesDistance += BoardWidth() + 1;
                        break;
                }
            }
        }

        return blackPiecesDistance - redPiecesDistance;
    }

    private EGameTileState?[][] MoveInCopy(int x, int y, EGameTileState?[][] board, bool currentPlayerBlack) 
    {
        var turnStays = false;
        var selectedTypeStandard = true;
        
        for (var i = 0; i < BoardWidth(); i++)
        {
            for (var j = 0; j < BoardHeight(); j++)
            {
                if (TileIsPossibleOrForcedInCopy(i, j, board))
                {
                    board[i][j] = null;
                }

                switch (board[i][j])
                {
                    case EGameTileState.SelectedBlackStandard:
                    {
                        board[x][y] = EGameTileState.BlackStandard;
                        
                        if (RedPieceOnTileInCopy((x + i) / 2, y - 1, board))
                        {
                            board[(x + i) / 2][y - 1] = null;
                            if (CanEatInCopy(x, y, board, currentPlayerBlack))
                            {
                                turnStays = true;
                            }
                        }
                        board[i][j] = null;
                        break;
                    }
                    case EGameTileState.SelectedRedStandard:
                    {
                        board[x][y] = EGameTileState.RedStandard;
                        
                        if (BlackPieceOnTileInCopy((x + i) / 2, y + 1, board))
                        {
                            board[(x + i) / 2][y + 1] = null;
                            if (CanEatInCopy(x, y, board, currentPlayerBlack))
                            {
                                turnStays = true;
                            }
                        }
                        board[i][j] = null;
                        break;
                    }
                    case EGameTileState.SelectedBlackKing:
                    {
                        selectedTypeStandard = false;
                        board[x][y] = EGameTileState.BlackKing;
                        
                        // Checks if BlackKing moved forward or backwards. If j > y, then he moved forward, otherwise backwards.
                        if (j > y)
                        {
                            if (RedPieceOnTileInCopy((x + i) / 2, y + 1, board))
                            {
                                board[(x + i) / 2][y + 1] = null;
                                if (CanEatInCopy(x, y, board, currentPlayerBlack))
                                {
                                    turnStays = true;
                                }
                            }
                        }
                        else
                        {
                            if (RedPieceOnTileInCopy((x + i) / 2, y - 1, board))
                            {
                                board[(x + i) / 2][y - 1] = null;
                                if (CanEatInCopy(x, y, board, currentPlayerBlack))
                                {
                                    turnStays = true;
                                }
                            }
                        }
                    
                        board[i][j] = null;
                        break;
                    }
                    case EGameTileState.SelectedRedKing:
                    {
                        selectedTypeStandard = false;
                        board[x][y] = EGameTileState.RedKing;

                        // Checks if RedKing moved forward or backwards. If j > y, then he moved backwards, otherwise forward.
                        if (j > y)
                        {
                            if (BlackPieceOnTileInCopy((x + i) / 2, y + 1, board))
                            {
                                board[(x + i) / 2][y + 1] = null;
                                if (CanEatInCopy(x, y, board, currentPlayerBlack))
                                {
                                    turnStays = true;
                                }
                            }
                        }
                    
                        else
                        {
                            if (BlackPieceOnTileInCopy((x + i) / 2, y - 1, board))
                            {
                                board[(x + i) / 2][y - 1] = null;
                                if (CanEatInCopy(x, y, board, currentPlayerBlack))
                                {
                                    turnStays = true;
                                }
                            }
                        }
                    
                        board[i][j] = null;
                        break;
                    }
                }
            }
        }
        
        if (y == 0 && !currentPlayerBlack)
        {
            board[x][y] = EGameTileState.RedKing;
        }
        
        else if (y == BoardHeight() - 1 && currentPlayerBlack)
        {
            board[x][y] = EGameTileState.BlackKing;
        }

        if (!turnStays)
        {
        }
        else
        {
            if (selectedTypeStandard)
            {
                board[x][y] = currentPlayerBlack ? EGameTileState.SelectedBlackStandard : EGameTileState.SelectedRedStandard;

            }
            else
            {
                board[x][y] = currentPlayerBlack ? EGameTileState.SelectedBlackKing : EGameTileState.SelectedRedKing;
            }
            ShowForcedMovesInCopy(x, y, board);
        }

        return board;
    }
    
    private bool PieceHasMovesInCopy(int x, int y, EGameTileState?[][] board, bool currentPlayerBlack) 
    {
        if (RedPieceOnTileInCopy(x, y, board) && !currentPlayerBlack)
        {
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                if (board[x - 1][y - 1] == null)
                {
                    return true;
                }
            }

            if (board.Length - 1 >= x + 1 && y - 1 >= 0)
            {
                if (board[x + 1][y - 1] == null)
                {
                    return true;
                }
            }

            if (RedKingOnTileInCopy(x, y, board))
            {
                if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                {
                    if (board[x - 1][y + 1] == null)
                    {
                        return true;
                    }
                }

                if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                {
                    if (board[x + 1][y + 1] == null)
                    {
                        return true;
                    }
                }
            }
        }
        
        else if (BlackPieceOnTileInCopy(x, y, board) && currentPlayerBlack)
        {
            
            if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
            {
                if (board[x - 1][y + 1] == null)
                {
                    return true;
                }
            }

            if (board.Length - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
            {
                if (board[x + 1][y + 1] == null)
                {
                    return true;
                }
            }

            if (BlackKingOnTileInCopy(x, y, board))
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    if (board[x - 1][y - 1] == null)
                    {
                        return true;
                    }
                }

                if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                {
                    if (board[x + 1][y - 1] == null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private EGameTileState?[][] ShowPossibleMovesInCopy(int x, int y, EGameTileState?[][] board)
    {
        if (RedPieceOnTileInCopy(x, y, board))
        {
            if (board[x][y] == EGameTileState.RedStandard)
            {
                board[x][y] = EGameTileState.SelectedRedStandard;
            }
            else
            {
                board[x][y] = EGameTileState.SelectedRedKing;
            }
            
            if (!GiveEatingOptionsIfCanEatForRedInCopy(x, y, board))
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    board[x - 1][y - 1] ??= EGameTileState.PossibleMove;
                }

                if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                {
                    board[x + 1][y - 1] ??= EGameTileState.PossibleMove;
                }

                if (board[x][y] == EGameTileState.SelectedRedKing)
                {
                    if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                    {
                        board[x - 1][y + 1] ??= EGameTileState.PossibleMove;
                    }

                    if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                    {
                        board[x + 1][y + 1] ??= EGameTileState.PossibleMove;
                    }
                }
            }
        }
        
        else if (BlackPieceOnTileInCopy(x, y, board))
        {
            if (board[x][y] == EGameTileState.BlackStandard)
            {
                board[x][y] = EGameTileState.SelectedBlackStandard;
            }
            else
            {
                board[x][y] = EGameTileState.SelectedBlackKing;
            }
            
            if (!GiveEatingOptionsIfCanEatForBlackInCopy(x, y, board))
            {
                if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                {
                    board[x - 1][y + 1] ??= EGameTileState.PossibleMove;
                }

                if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                {
                    board[x + 1][y + 1] ??= EGameTileState.PossibleMove;
                }

                if (board[x][y] == EGameTileState.SelectedBlackKing)
                {
                    if (x - 1 >= 0 && y - 1 >= 0)
                    {
                        board[x - 1][y - 1] ??= EGameTileState.PossibleMove;
                    }

                    if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                    {
                        board[x + 1][y - 1] ??= EGameTileState.PossibleMove;
                    }
                }
            }
        }

        return board;
    }

    private void ShowForcedMovesInCopy(int x, int y, EGameTileState?[][] board)
    {
        if (SelectedRedPieceOnTileInCopy(x, y, board))
        {
            if (!GiveForcedEatingOptionsForRedInCopy(x, y, board))
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    board[x - 1][y - 1] ??= EGameTileState.ForcedMove;
                }

                if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                {
                    board[x + 1][y - 1] ??= EGameTileState.ForcedMove;
                }

                if (board[x][y] == EGameTileState.SelectedRedKing)
                {
                    if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                    {
                        board[x - 1][y + 1] ??= EGameTileState.ForcedMove;
                    }

                    if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                    {
                        board[x + 1][y + 1] ??= EGameTileState.ForcedMove;
                    }
                }
            }
        }

        else
        {
            if (!GiveForcedEatingOptionsForBlackInCopy(x, y, board))
            {
                if (x - 1 >= 0 && BoardHeight() - 1 >= y + 1)
                {
                    board[x - 1][y + 1] ??= EGameTileState.ForcedMove;
                }

                if (BoardWidth() - 1 >= x + 1 && BoardHeight() - 1 >= y + 1)
                {
                    board[x + 1][y + 1] ??= EGameTileState.ForcedMove;
                }

                if (board[x][y] == EGameTileState.SelectedBlackKing)
                {
                    if (x - 1 >= 0 && y - 1 >= 0)
                    {
                        board[x - 1][y - 1] ??= EGameTileState.ForcedMove;
                    }

                    if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
                    {
                        board[x + 1][y - 1] ??= EGameTileState.ForcedMove;
                    }
                }
            }
        }
    }
    
    private bool GiveEatingOptionsIfCanEatForRedInCopy(int x, int y, EGameTileState?[][] board)
    {
        var canEat = false;
        
        if (x - 1 >= 0 && y - 1 >= 0)
        {
            if (BlackPieceOnTileInCopy(x - 1, y - 1, board) && x - 2 >= 0 && y - 2 >= 0)
            {
                if (board[x - 2][y - 2] == null)
                {
                    board[x - 2][y - 2] = EGameTileState.PossibleMove;
                    canEat = true;
                }
            }
        }

        if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
        {
            if (BlackPieceOnTileInCopy(x + 1, y - 1, board) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
            {
                if (board[x + 2][y - 2] == null)
                {
                    board[x + 2][y - 2] = EGameTileState.PossibleMove;
                    canEat = true;
                }
            }
        }

        if (board[x][y] == EGameTileState.SelectedRedKing)
        {
            if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
            {
                if (BlackPieceOnTileInCopy(x - 1, y + 1, board) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
                { 
                    if (board[x - 2][y + 2] == null)
                    {
                        board[x - 2][y + 2] = EGameTileState.PossibleMove;
                        canEat = true;
                    }
                }
            }

            if (x + 1 <= BoardWidth() - 1 && y + 1 <= BoardHeight() - 1)
            {
                if (BlackPieceOnTileInCopy(x + 1, y + 1, board) && x + 2 <= BoardWidth() - 1 && y + 2 <= BoardHeight() - 1)
                {
                    if (board[x + 2][y + 2] == null)
                    { 
                        board[x + 2][y + 2] = EGameTileState.PossibleMove;
                        canEat = true;
                    }
                }
            }
        }

        return canEat;
    }

    private bool GiveEatingOptionsIfCanEatForBlackInCopy(int x, int y, EGameTileState?[][] board)
    {
        var canEat = false;

        if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
        {
            if (RedPieceOnTileInCopy(x - 1, y + 1, board) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
            { 
                if (board[x - 2][y + 2] == null)
                {
                    board[x - 2][y + 2] = EGameTileState.PossibleMove;
                    canEat = true;
                }
            }
        }

        if (x + 1 <= BoardWidth() - 1 && y + 1 <= BoardHeight() - 1)
        {
            if (RedPieceOnTileInCopy(x + 1, y + 1, board) && x + 2 <= BoardWidth() - 1 && y + 2 <= BoardHeight() - 1)
            {
                if (board[x + 2][y + 2] == null)
                { 
                    board[x + 2][y + 2] = EGameTileState.PossibleMove;
                    canEat = true;
                }
            }
        }

        if (board[x][y] == EGameTileState.SelectedBlackKing)
        {
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                if (RedPieceOnTileInCopy(x - 1, y - 1, board) && x - 2 >= 0 && y - 2 >= 0)
                {
                    if (board[x - 2][y - 2] == null)
                    {
                        board[x - 2][y - 2] = EGameTileState.PossibleMove;
                        canEat = true;
                    }
                }
            }

            if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
            {
                if (RedPieceOnTileInCopy(x + 1, y - 1, board) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
                {
                    if (board[x + 2][y - 2] == null)
                    {
                        board[x + 2][y - 2] = EGameTileState.PossibleMove;
                        canEat = true;
                    }
                }
            }
        }
        
        return canEat;
    }

    private bool JumpsAvailableWithoutForcedInCopy(EGameTileState?[][] board, bool currentPlayerBlack)
    {
        if (currentPlayerBlack)
        {
            for (var i = 0; i < BoardWidth(); i++)
            {
                for (var j = 0; j < BoardHeight(); j++)
                {
                    if (BlackPieceOnTileInCopy(i, j, board) || SelectedBlackPieceOnTileInCopy(i, j, board))
                    {
                        if (CanEatInCopy(i, j, board, currentPlayerBlack))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        else
        {
            for (var i = 0; i < BoardWidth(); i++)
            {
                for (int j = 0; j < BoardHeight(); j++)
                {
                    if (RedPieceOnTileInCopy(i, j, board) || SelectedRedPieceOnTileInCopy(i, j, board))
                    {
                        if (CanEatInCopy(i, j, board, currentPlayerBlack))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool ExistenceOfForcedJumpsInCopy(EGameTileState?[][] board)
    {
        for (var i = 0; i < BoardWidth(); i++)
        {
            for (var j = 0; j < BoardHeight(); j++)
            {
                if (board[i][j] == EGameTileState.ForcedMove)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool GiveForcedEatingOptionsForBlackInCopy(int x, int y, EGameTileState?[][] board)
    {
        var canEat = false;

        if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
        {
            if (RedPieceOnTileInCopy(x - 1, y + 1, board) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
            { 
                if (board[x - 2][y + 2] == null)
                {
                    board[x - 2][y + 2] = EGameTileState.ForcedMove;
                    canEat = true;
                }
            }
        }

        if (x + 1 <= BoardWidth() - 1 && y + 1 <= BoardHeight() - 1)
        {
            if (RedPieceOnTileInCopy(x + 1, y + 1, board) && x + 2 <= BoardWidth() - 1 && y + 2 <= BoardHeight() - 1)
            {
                if (board[x + 2][y + 2] == null)
                { 
                    board[x + 2][y + 2] = EGameTileState.ForcedMove;
                    canEat = true;
                }
            }
        }

        if (board[x][y] == EGameTileState.SelectedBlackKing)
        {
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                if (RedPieceOnTileInCopy(x - 1, y - 1, board) && x - 2 >= 0 && y - 2 >= 0)
                {
                    if (board[x - 2][y - 2] == null)
                    {
                        board[x - 2][y - 2] = EGameTileState.ForcedMove;
                        canEat = true;
                    }
                }
            }

            if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
            {
                if (RedPieceOnTileInCopy(x + 1, y - 1, board) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
                {
                    if (board[x + 2][y - 2] == null)
                    {
                        board[x + 2][y - 2] = EGameTileState.ForcedMove;
                        canEat = true;
                    }
                }
            }
        }
        
        return canEat;
    }

    private bool GiveForcedEatingOptionsForRedInCopy(int x, int y, EGameTileState?[][] board)
    {
        var canEat = false;
        
        if (x - 1 >= 0 && y - 1 >= 0)
        {
            if (BlackPieceOnTileInCopy(x - 1, y - 1, board) && x - 2 >= 0 && y - 2 >= 0)
            {
                if (board[x - 2][y - 2] == null)
                {
                    board[x - 2][y - 2] = EGameTileState.ForcedMove;
                    canEat = true;
                }
            }
        }

        if (BoardWidth() - 1 >= x + 1 && y - 1 >= 0)
        {
            if (BlackPieceOnTileInCopy(x + 1, y - 1, board) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
            {
                if (board[x + 2][y - 2] == null)
                {
                    board[x + 2][y - 2] = EGameTileState.ForcedMove;
                    canEat = true;
                }
            }
        }

        if (board[x][y] == EGameTileState.SelectedRedKing)
        {
            if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
            {
                if (BlackPieceOnTileInCopy(x - 1, y + 1, board) && x - 2 >= 0 &&
                    y + 2 <= BoardHeight() - 1)
                { 
                    if (board[x - 2][y + 2] == null)
                    {
                        board[x - 2][y + 2] = EGameTileState.ForcedMove;
                        canEat = true;
                    }
                }
            }

            if (x + 1 <= BoardWidth() - 1 && y + 1 <= BoardHeight() - 1)
            {
                if (BlackPieceOnTileInCopy(x + 1, y + 1, board) && x + 2 <= BoardWidth() - 1 &&
                    y + 2 <= BoardHeight() - 1)
                {
                    if (board[x + 2][y + 2] == null)
                    { 
                        board[x + 2][y + 2] = EGameTileState.ForcedMove;
                        canEat = true;
                    }
                }
            }
        }

        return canEat;
    }
    
    private bool CanEatInCopy(int x, int y, EGameTileState?[][] board, bool nextPlayerBlack)
    {
        if (nextPlayerBlack)
        {
            if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
            {
                if (RedPieceOnTileInCopy(x - 1, y + 1, board) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
                {
                    if (board[x - 2][y + 2] == null || board[x - 2][y + 2] == EGameTileState.PossibleMove)
                    {
                        return true;
                    }
                }
            }

            if (BoardWidth() - 1 >= x + 1 && y + 1 <= BoardHeight() - 1)
            {
                if (RedPieceOnTileInCopy(x + 1, y + 1, board) && x + 2 <= board.Length - 1 && y + 2 <= board[0].Length - 1)
                {
                    if (board[x + 2][y + 2] == null || board[x + 2][y + 2] == EGameTileState.PossibleMove)
                    {
                        return true;
                    }
                }
            }

            if (BlackKingOnTileInCopy(x, y, board))
            {
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    if (RedPieceOnTileInCopy(x - 1, y - 1, board) && x - 2 >= 0 && y - 2 >= 0)
                    {
                        if (board[x - 2][y - 2] == null || board[x - 2][y - 2] == EGameTileState.PossibleMove)
                        {
                            return true;
                        }
                    }
                }
                if (x + 1 <= BoardWidth() - 1 && y - 1 >= 0)
                {
                    if (RedPieceOnTileInCopy(x + 1, y - 1, board) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
                    {
                        if (board[x + 2][y - 2] == null || board[x + 2][y - 2] == EGameTileState.PossibleMove)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        else
        {
            if (x - 1 >= 0 && y - 1 >= 0)
            {
                if (BlackPieceOnTileInCopy(x - 1, y - 1, board) && x - 2 >= 0 && y - 2 >= 0)
                {
                    if (board[x - 2][y - 2] == null || board[x - 2][y - 2] == EGameTileState.PossibleMove)
                    {
                        return true;
                    }
                }
            }
            if (x + 1 <= BoardWidth() - 1 && y - 1 >= 0)
            {
                if (BlackPieceOnTileInCopy(x + 1, y - 1, board) && x + 2 <= BoardWidth() - 1 && y - 2 >= 0)
                {
                    if (board[x + 2][y - 2] == null || board[x + 2][y - 2] == EGameTileState.PossibleMove)
                    {
                        return true;
                    }
                }
            }

            if (RedKingOnTileInCopy(x, y, board))
            {
                if (x - 1 >= 0 && y + 1 <= BoardHeight() - 1)
                {
                    if (BlackPieceOnTileInCopy(x - 1, y + 1, board) && x - 2 >= 0 && y + 2 <= BoardHeight() - 1)
                    {
                        if (board[x - 2][y + 2] == null || board[x - 2][y + 2] == EGameTileState.PossibleMove)
                        {
                            return true;
                        }
                    }
                }

                if (BoardWidth() - 1 >= x + 1 && y + 1 <= BoardHeight() - 1)
                {
                    if (BlackPieceOnTileInCopy(x + 1, y + 1, board) && x + 2 <= BoardWidth() - 1 && y + 2 <= BoardHeight() - 1)
                    {
                        if (board[x + 2][y + 2] == null || board[x + 2][y + 2] == EGameTileState.PossibleMove)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }
    
    private bool CheckIfGameOver(EGameTileState?[][] board, bool currentPlayerBlack)
    {
        for(var x = 0; x < BoardWidth(); x++) 
        {
            for (var y = 0; y < BoardHeight(); y++)
            {
                if (TileIsPossibleOrForcedInCopy(x, y, board))
                {
                    return false;
                }
            }
        }
    
        if (!ExistenceOfForcedJumpsInCopy(board))
        {
            for (var x = 0; x < BoardWidth(); x++)
            {
                var diff = x % 2 == 0 ? 1 : 0;

                for (var y = diff; y < BoardHeight(); y += 2)
                {
                    if (JumpsAvailableWithoutForcedInCopy(board, currentPlayerBlack))
                    {
                        return false;
                    }
                
                    if (PieceHasMovesInCopy(x, y, board, currentPlayerBlack))
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            return false;
        }
        return true;
    }
}
