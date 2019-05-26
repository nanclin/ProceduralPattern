using UnityEngine;

class QuadTree {

    public bool IsLeaf{ get; private set; }

    public int Depth{ get; private set; }

    public QuadTree Root;
    public QuadTree Parent;
    public QuadTree[] Leafes;
    public Vector2 Pos;
    public float Size;
    public int TreeHeight;

    public QuadTree(int depth = 0, int quadrant = 0, QuadTree root = null, QuadTree parent = null) {
        Depth = depth;
        Root = root;
        Parent = parent;

        IsLeaf = true;
        Leafes = null;

        if (Depth == 0) {
            Root = this;
            Root.TreeHeight = 1;
        }
        TreeHeight = Root.TreeHeight;

        Size = 1.0f / Mathf.Pow(2.0f, Depth);
        Pos = Vector2.zero;

        /*
        * q2 | q3
        * -------
        * q0 | q1
        */
        if (depth > 0) {
            switch (quadrant) {
                case 0:
                    Pos = new Vector2(-Size, -Size);
                    break;
                case 1:
                    Pos = new Vector2(Size, -Size);
                    break;
                case 2:
                    Pos = new Vector2(-Size, Size);
                    break;
                case 3:
                    Pos = new Vector2(Size, Size);
                    break;
            }
            Pos *= 0.5f;
            if (parent != null)
                Pos += parent.Pos;
        }
    }

    public void Split() {
        Leafes = new QuadTree[4];
        Leafes[0] = new QuadTree(Depth + 1, 0, Root, this);
        Leafes[1] = new QuadTree(Depth + 1, 1, Root, this);
        Leafes[2] = new QuadTree(Depth + 1, 2, Root, this);
        Leafes[3] = new QuadTree(Depth + 1, 3, Root, this);
        IsLeaf = false;

        if (Depth + 1 > Root.TreeHeight) {
            Root.TreeHeight = Depth + 1;
            TreeHeight = Depth + 1;
        }
    }

    public QuadTree GetLeaveAtPosition(Vector2 pos) {
        Bounds bounds = new Bounds(Pos, Vector3.one * Size);
        if (bounds.Contains(pos)) {
            if (IsLeaf) return this;
            else {
                QuadTree tree;
                tree = Leafes[0].GetLeaveAtPosition(pos);
                if (tree != null) return tree;
                tree = Leafes[1].GetLeaveAtPosition(pos);
                if (tree != null) return tree;
                tree = Leafes[2].GetLeaveAtPosition(pos);
                if (tree != null) return tree;
                tree = Leafes[3].GetLeaveAtPosition(pos);
                if (tree != null) return tree;
            }
        }
        return null;
    }
}

public class GameController : MonoBehaviour {

    [SerializeField] private int MaxDepth = 5;
    [SerializeField] private int BrushFrameSkip = 5;

    private QuadTree Tree;

    // Use this for initialization
    void Start() {
        GeneratePattern();
    }

    private void GeneratePattern() {
        // new tree
        Tree = new QuadTree();
//        Tree.Split();
//        Tree.Leafes[0].Split();
//        Tree.Leafes[0].Leafes[2].Split();
    }
	
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            GeneratePattern();
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(0) && Time.frameCount % BrushFrameSkip == 0) {
            QuadTree clickedTree = Tree.GetLeaveAtPosition(mousePos);
            if (clickedTree != null && clickedTree.Depth < MaxDepth)
                clickedTree.Split();
        }
    }

    void OnDrawGizmos() {
        if (!Application.isPlaying) return;
        DrawQuadTreeLevel(Tree);
    }

    void DrawQuadTreeLevel(QuadTree tree) {
        if (tree.TreeHeight <= 0) return;

        Gizmos.color = Color.Lerp(Color.white, Color.white * 0.5f, (float) tree.Depth / (float) tree.TreeHeight);
        Gizmos.DrawWireCube(tree.Pos, Vector3.one * tree.Size);

        if (!tree.IsLeaf) {
            DrawQuadTreeLevel(tree.Leafes[0]);
            DrawQuadTreeLevel(tree.Leafes[1]);
            DrawQuadTreeLevel(tree.Leafes[2]);
            DrawQuadTreeLevel(tree.Leafes[3]);
        }
    }
}
