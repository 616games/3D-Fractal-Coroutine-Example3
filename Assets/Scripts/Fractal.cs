using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fractal : MonoBehaviour
{
    #region --Fields / Properties--

    /// <summary>
    /// Chance for a child branch to spawn to create variability in the fractal's appearance.
    /// </summary>
    [SerializeField]
    private float _spawnProbability;

    /// <summary>
    /// Limits how fast each child branch can rotate.
    /// </summary>
    [SerializeField]
    private float _maxRotationSpeed;

    /// <summary>
    /// A random twist is applied when each child is spawned to add more variety to the overall appearance.
    /// </summary>
    [SerializeField]
    private float _maxTwist;
    
    /// <summary>
    /// Mesh to be used by each fractal.
    /// Add duplicates to the array to increase the chance the mesh is chosen to spawn.
    /// </summary>
    [SerializeField]
    private Mesh[] _meshes;

    /// <summary>
    /// Material to be used by each fractal.
    /// </summary>
    [SerializeField]
    private Material _material;

    /// <summary>
    /// The current number of layers deep into the fractal.
    /// </summary>
    [SerializeField]
    private int _maxDepth = 4;

    /// <summary>
    /// The scale of each child fractal.
    /// </summary>
    [SerializeField]
    private float _childScale;

    /// <summary>
    /// The current depth of the child fractal being created, not to reach more than _maxDepth.
    /// </summary>
    private int _depth;

    /// <summary>
    /// Cached Transform component.
    /// </summary>
    private Transform _transform;

    /// <summary>
    /// An array of 5 directions used by each child fractal.
    /// </summary>
    private readonly Vector3[] _directions = { Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

    /// <summary>
    /// An array of rotations that correspond to each of the 5 directions used by each child fractal.
    /// </summary>
    private readonly Quaternion[] _orientations = { Quaternion.identity, Quaternion.Euler(0, 0, -90f), Quaternion.Euler(0, 0, 90f), Quaternion.Euler(-90f, 0, 0), Quaternion.Euler(-90f, 0, 0) };

    /// <summary>
    /// Holds a material for each level of depth to bring back dynamic batching.
    /// </summary>
    private Material[,] _materials;

    /// <summary>
    /// Rotation speed to use for a given child branch.
    /// </summary>
    private float _rotationSpeed;
    
    #endregion
    
    #region --Unity Specific Methods--

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        Rotate();
    }

    #endregion
    
    #region --Custom Methods--

    /// <summary>
    /// Initializes variables and caches components.
    /// </summary>
    private void Init()
    {
        if(transform != null && _transform != transform) _transform = transform;
        
        if(_materials == null) InitializeMaterials();

        _rotationSpeed = Random.Range(-_maxRotationSpeed, _maxRotationSpeed);
        _transform.Rotate(Random.Range(-_maxTwist, _maxTwist), 0, 0);
        
        gameObject.AddComponent<MeshFilter>().mesh = _meshes[Random.Range(0, _meshes.Length)];
        gameObject.AddComponent<MeshRenderer>().material = _materials[_depth, Random.Range(0, 2)];
        
        //This check prevents Unity from crashing since we will be creating instances of this class repeatedly.
        if (_depth < _maxDepth)
        {
            //Start is not called on these new game objects until the next frame after Fractal is added as a component.
            //This means none of the code here in Start prior to its creation will be applied immediately - keep that in mind.
            StartCoroutine(CreateFractalChildren());
        }
    }

    /// <summary>
    /// Adds color variety to the child fractals per layer of depth.
    /// </summary>
    private void InitializeMaterials()
    {
        _materials = new Material[_maxDepth + 1, 2];
        for (int i = 0; i <= _maxDepth; i++)
        {
            float _t = i / (_maxDepth - 1f);
            _t *= _t;
            _materials[i, 0] = new Material(_material);
            _materials[i, 0].color = Color.Lerp(Color.white, Color.yellow, _t);
            _materials[i, 1] = new Material(_material);
            _materials[i, 1].color = Color.Lerp(Color.white, Color.cyan, _t);
        }

        _materials[_maxDepth, 0].color = Color.magenta;
        _materials[_maxDepth, 1].color = Color.red;
    }
    
    /// <summary>
    /// Rotates the fractal and applies twist.
    /// </summary>
    private void Rotate()
    {
        _transform.Rotate(0, _rotationSpeed * Time.deltaTime, 0f);
    }

    /// <summary>
    /// Creates the child game objects for each parent.
    /// Formula for number of children:  f(0) = 1, f(n) = 5 * f(n - 1) + 1
    /// </summary>
    private IEnumerator CreateFractalChildren()
    {
        for (int i = 0; i < _directions.Length; i++)
        {
            if (!(Random.value < _spawnProbability)) continue;

            yield return new WaitForSeconds(Random.Range(.1f, .5f));
            new GameObject("Fractal Child").AddComponent<Fractal>().CreateFractalInstance(this, i);
        }
    }
    
    /// <summary>
    /// Create an instance of the fractal that takes the components and variables from its parent in the Hierarchy.
    /// </summary>
    private void CreateFractalInstance(Fractal _parentFractal, int _childIndex)
    {
        _transform = transform;
        _meshes = _parentFractal._meshes;
        _materials = _parentFractal._materials;
        _material = _parentFractal._material;
        _maxDepth = _parentFractal._maxDepth;
        _depth = _parentFractal._depth + 1;
        _childScale = _parentFractal._childScale;
        _spawnProbability = _parentFractal._spawnProbability;
        _maxRotationSpeed = _parentFractal._maxRotationSpeed;
        _maxTwist = _parentFractal._maxTwist;
        _transform.SetParent(_parentFractal._transform);
        _transform.localScale = Vector3.one * _childScale;
        
        //Move each child by .5f in the direction specified and again by half its scale.
        _transform.localPosition = _directions[_childIndex] * (.5f + .5f * _childScale);
        _transform.localRotation = _orientations[_childIndex];
    }
    
    #endregion
    
}
