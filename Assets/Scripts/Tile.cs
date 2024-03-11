using UnityEngine;

public class Tile : MonoBehaviour
{
	public int x;
	public int y;
	public Board board;

	public void Setup(int x, int y, Board board)
	{
		this.x = x;
		this.y = y;
		this.board = board;
	}

	private void OnMouseDown()
	{
		board.TileDown(this);
	}

	private void OnMouseEnter()
	{
		board.TileOver(this);
	}

	private void OnMouseUp()
	{
		board.TileUp(this);
	}
}
