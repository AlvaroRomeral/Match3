using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Piece : MonoBehaviour
{
	public int x;
	public int y;
	public Board board;

	public enum types
	{
		ELEPHANT,
		GIRAFFE,
		HIPPO,
		MONKEY,
		PANDA,
		PARROT,
		PENGUIN,
		PIG,
		RABBIT,
		SNAKE,
	}
	public types type;

	public Piece Left => x > 0 ? board.Pieces[x - 1, y] : null;
	public Piece Top => y > 0 ? board.Pieces[x, y - 1] : null;
	public Piece Rigth => x < board.width - 1 ? board.Pieces[x + 1, y] : null;
	public Piece Botton => y < board.height - 1 ? board.Pieces[x, y + 1] : null;

	public Piece[] Neighbours => new[]
	{
		Left,
		Top,
		Rigth,
		Botton,
	};

	public void Setup(int x, int y, Board board)
	{
		this.x = x;
		this.y = y;
		this.board = board;
	}

	public void Move(int desx, int desy)
	{
		transform.DOMove(new Vector3(desx, desy, -5f), 0.25f).SetEase(Ease.InOutCubic).onComplete = () =>
		{
			x = desx;
			y = desy;
		};
	}

	// [ContextMenu("Test Move")]
	// public void TestMove()
	// {
	// 	Move(0, 0);
	// }

	public List<Piece> GetConnectedTiles(List<Piece> exclude = null)
	{
		var result = new List<Piece> { this, };

		if (exclude == null)
		{
			exclude = new List<Piece> { this, };
		}
		else
		{
			exclude.Add(this);
		}

		foreach (var neighbour in Neighbours)
		{
			if (neighbour == null || exclude.Contains(neighbour) || neighbour.type != type) continue;

			result.AddRange(neighbour.GetConnectedTiles(exclude));
		}

		return result;
	}
}
