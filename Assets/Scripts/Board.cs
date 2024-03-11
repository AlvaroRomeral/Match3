using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Board : MonoBehaviour
{
	public int width;
	public int height;
	public GameObject tileObject;

	public float cameraSizeOffset;
	public float cameraVerticalOffset;
	public float tweenDuration;

	public GameObject[] avalaiblePieces;

	public Tile[,] Tiles;
	public Piece[,] Pieces;

	Tile startTile;
	Tile endTile;

	private int score = 0;
	public TextMeshProUGUI textScore;


	void Start()
	{
		Tiles = new Tile[width, height];
		Pieces = new Piece[width, height];
		SetupBoard();
		PositionCamera();
		SetupPieces();
	}

	private void PositionCamera()
	{
		float newPosX = width / 2f;
		float newPosY = height / 2f;
		Camera.main.transform.position = new Vector3(newPosX - 0.5f, newPosY - 0.5f + cameraVerticalOffset, -10f);

		float horizontal = width + 1;
		float vertical = (height / 2) + 1;

		Camera.main.orthographicSize = horizontal > vertical ? horizontal + cameraSizeOffset : vertical;
	}

	private void SetupBoard()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				var o = Instantiate(tileObject, new Vector3(x, y, -5), Quaternion.identity);
				o.transform.parent = transform;
				Tiles[x, y] = o.GetComponent<Tile>();
				Tiles[x, y]?.Setup(x, y, this);
			}

		}
	}

	private void SetupPieces()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				while(true)
				{
					if (Pieces[x, y])
						Destroy(Pieces[x, y].gameObject);
					Pieces[x, y] = null;
					
					var selectedPiece = avalaiblePieces[UnityEngine.Random.Range(0, avalaiblePieces.Length)];
					var o = Instantiate(selectedPiece, new Vector3(x, y, -5), Quaternion.identity);
					o.transform.parent = transform;
					Pieces[x, y] = o.GetComponent<Piece>();
					Pieces[x, y]?.Setup(x, y, this);

					if (!CanPop()) break;
				}
			}

		}
	}

	public void TileDown(Tile tile_)
	{
		startTile = tile_;
	}

	public void TileOver(Tile tile_)
	{
		endTile = tile_;
	}

	public void TileUp(Tile tile_)
	{
		if (startTile != null && endTile != null && IsCloseTo(startTile, endTile))
		{
			SwapTiles();
			
			if (CanPop())
			{
				Pop();
			}
			else
			{
				// En caso de que quiera que no sea posible mover fichas si no hay posibilidad de hacer Pop
			}
		}
	}

	private void SwapTiles()
	{
		var StartPiece = Pieces[startTile.x, startTile.y];
		var EndPiece = Pieces[endTile.x, endTile.y];

		StartPiece.Move(endTile.x, endTile.y);
		EndPiece.Move(startTile.x, startTile.y);

		Pieces[startTile.x, startTile.y] = EndPiece;
		Pieces[endTile.x, endTile.y] = StartPiece;
	}

	public bool IsCloseTo(Tile start, Tile end)
	{
		if (Math.Abs(start.x - end.x) == 1 && start.y == end.y)
			return true;
		if (Math.Abs(start.y - end.y) == 1 && start.x == end.x)
			return true;
		return false;
	}

	private bool CanPop()
	{
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (Pieces[x, y])
					if (Pieces[x, y].GetConnectedTiles().Skip(1).Count() >= 2) return true;
			}
		}
		return false;
	}

	private async void Pop()
	{
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var piece = Pieces[x, y];

				var connectedPieces = piece.GetConnectedTiles();
				
				if (connectedPieces.Skip(1).Count() < 2) continue; // Si no hay mas de 2 piezas conectadas pasa, si lo hay, continua

				var deflateSequence = DOTween.Sequence();

				foreach (var connectedPiece in connectedPieces)
				{
					deflateSequence.Join(connectedPiece.transform.DOScale(Vector3.zero, tweenDuration));
				}

				await deflateSequence.Play().AsyncWaitForCompletion();

				foreach (var connectedPiece in connectedPieces)
				{
					var x_ = connectedPiece.x;
					var y_ = connectedPiece.y;

					Pieces[x_, y_] = null;

					Destroy(connectedPiece.gameObject);

					AddScore(3); // Añadir diferentes puntuaciones dependiendo del animal Ej. conectedPiece.scorePoints
				}

				// DropPieces();

				await RefillTiles();
			}
		}
	}

	private async void DropPieces()
	{
		bool pieceMoved = false;

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var piece = Pieces[x, y];
				if (piece)
				{
					if (piece.Botton == null && x != 0)
					{
						piece.Move(x, y - 1);
						pieceMoved = true;
					}
				}

			}
		}

		if (pieceMoved)
		{
			DropPieces();
		}
		else
		{
			await RefillTiles();
		}
	}

	private async Task RefillTiles()
	{
		var inflateSequence = DOTween.Sequence();
		
		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				if (Pieces[x, y] == null)
				{
					while(true)
					{
						if (Pieces[x, y])
							Destroy(Pieces[x, y].gameObject);
						Pieces[x, y] = null;
						
						var selectedPiece = avalaiblePieces[UnityEngine.Random.Range(0, avalaiblePieces.Length)];
						var o = Instantiate(selectedPiece, new Vector3(x, y, -5), Quaternion.identity);
						o.transform.parent = transform;
						Pieces[x, y] = o.GetComponent<Piece>();
						Pieces[x, y]?.Setup(x, y, this);

						if (!CanPop()) break;
					}
				}
			}
		}

		await inflateSequence.Play().AsyncWaitForCompletion();
	}

	private void AddScore(int _score)
	{
		score += _score;
		textScore.text = "Score: " + score.ToString("000#");
	}

	
}