using Assets.General.DataStructures;
using Assets.General.UnityExtensions;
using Assets.General;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.Rendering;

public class WhereToMoveState : BattleState
{
    private Unit SelectedUnit;
    private GameTile InitialUnitTile;
    private HashSet<Vector2Int> MovementTiles;
    private LinkedList<GameTile> TilesToPass;
    private CommandBuffer buf = new CommandBuffer();

    public static BattleState Create( Unit selectedUnit )
    {
        return new ControlCursorState( new CancelableState( new WhereToMoveState( selectedUnit ) ) );
    }

    private WhereToMoveState( Unit selectedUnit )
    {
        SelectedUnit = selectedUnit;
        InitialUnitTile = sys.Map.UnitGametileMap[ SelectedUnit ];
    }

    public override void Update( BattleSystem sys )
    {
        RenderForPath( TilesToPass );
        Unit unitUnderCursor = null;
        sys.Map.UnitGametileMap.TryGetValue( sys.Cursor.CurrentTile, out unitUnderCursor );

        if ( Input.GetButtonDown( "Submit" ) )
        {
            bool canMoveHere = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
            if ( canMoveHere )
            {
                sys.CurrentTurn.DoCommand( 
                    CreateMoveCommand( sys.Cursor.CurrentTile, InitialUnitTile ) );
                sys.TurnState = ChoosingUnitActionsState.Create( SelectedUnit );
            }
        }
    }

    public override void Enter( BattleSystem sys )
    {
        sys.Cursor.CursorMoved.AddListener( CursorMoved );

        TilesToPass = new LinkedList<GameTile>();
        TilesToPass.AddFirst( InitialUnitTile );
        MovementTiles = new HashSet<Vector2Int>( sys.Map.GetValidMovementPositions( SelectedUnit, InitialUnitTile ) );
        Camera.main.AddCommandBuffer( CameraEvent.AfterSkybox, buf );
    }

    public override void Exit( BattleSystem sys )
    {
        sys.Cursor.CursorMoved.RemoveListener( CursorMoved );
        sys.Cursor.MoveCursor( sys.Map.UnitGametileMap[ SelectedUnit ].Position );
        Camera.main.RemoveAllCommandBuffers();
    }

    private UndoCommandAction CreateMoveCommand( GameTile targetTile, GameTile initialTile )
    {
        return new UndoCommandAction(
            delegate
            {
                SelectedUnit.StartCoroutine(
                    CoroutineHelper.AddActions( 
                        CustomAnimation.InterpolateBetweenPointsDecoupled( SelectedUnit.transform,
                        SelectedUnit.transform.FindChild( "Model" ),
                        TilesToPass.Select( x => x.GetComponent<Transform>().localPosition ).Reverse().ToList(), 0.22f ),
                        () => SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", true ),
                        () => SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false ) ) );

                sys.Map.PlaceUnit( SelectedUnit, targetTile );
            },
            delegate
            {
                SelectedUnit.GetComponentInChildren<Animator>().SetBool( "Moving", false );
                SelectedUnit.StopAllCoroutines();

                sys.Map.PlaceUnit( SelectedUnit, initialTile );
                SelectedUnit.transform.position = initialTile.transform.position;

                sys.Cursor.MoveCursor( initialTile.Position );
                sys.Map.ShowUnitMovement( SelectedUnit );
            } );
    }

    private void CursorMoved()
    {
        bool withinMoveRange = MovementTiles.Contains( sys.Cursor.CurrentTile.Position );
        if ( withinMoveRange )
        {
            AttemptToLengthenPath( sys.Cursor.CurrentTile );
        }
    }

    public void RenderForPath( IEnumerable<GameTile> tilesToPass )
    {
        buf.Clear();
        foreach ( var tile in tilesToPass )
        {
            buf.DrawRenderer( tile.GetComponent<Renderer>(), sys.Map.SelectionMat );
        }
    }

    //public void RenderForPath( IEnumerable<GameTile> tilesToPass )
    //{
    //    foreach ( var tile in tilesToPass )
    //    {
    //        sys.Map.DefaultMat.SetPass( -1 );
    //        Graphics.DrawMeshNow( tile.GetComponent<MeshFilter>().sharedMesh,
    //            sys.Map.transform.localToWorldMatrix *
    //            Matrix4x4.TRS( new Vector3( 0.5f, 0f, 0.5f ), Quaternion.identity, Vector3.one ) *
    //            Matrix4x4.TRS( tile.transform.localPosition, Quaternion.identity, tile.transform.localScale ) );
    //    }
    //}

    private void AttemptToLengthenPath( GameTile to )
    {
        bool tooFarFromLast = false;
        if ( TilesToPass.Count > 0 )
            tooFarFromLast = TilesToPass.First.Value.Position
                .ManhattanDistance( to.Position ) > 1;

        if ( TilesToPass.Count > SelectedUnit.MovementRange || tooFarFromLast )
        {
            TilesToPass = new LinkedList<GameTile>( MapSearcher.Search( InitialUnitTile, to, sys.Map, SelectedUnit.MovementRange ) );
            return;
        }

        LinkedListNode<GameTile> alreadyPresent = TilesToPass.Find( to );
        if ( alreadyPresent == null )
        {
            TilesToPass.AddFirst( to );
        }
        else
        {
            while ( TilesToPass.First != alreadyPresent )
            {
                TilesToPass.RemoveFirst();
            }
        }
    }
}