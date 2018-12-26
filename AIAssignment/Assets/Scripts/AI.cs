using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************************************************************
 * Write your core AI code in this file here. The private variable 'agentScript' contains all the agents actions which are listed
 * below. Ensure your code it clear and organised and commented.
 *
 * Unity Tags
 * public static class Tags
 * public const string BlueTeam = "Blue Team";	The tag assigned to blue team members.
 * public const string RedTeam = "Red Team";	The tag assigned to red team members.
 * public const string Collectable = "Collectable";	The tag assigned to collectable items (health kit and power up).
 * public const string Flag = "Flag";	The tag assigned to the flags, blue or red.
 * 
 * Unity GameObject names
 * public static class Names
 * public const string PowerUp = "Power Up";	Power up name
 * public const string HealthKit = "Health Kit";	Health kit name.
 * public const string BlueFlag = "Blue Flag";	The blue teams flag name.
 * public const string RedFlag = "Red Flag";	The red teams flag name.
 * public const string RedBase = "Red Base";    The red teams base name.
 * public const string BlueBase = "Blue Base";  The blue teams base name.
 * public const string BlueTeamMember1 = "Blue Team Member 1";	Blue team member 1 name.
 * public const string BlueTeamMember2 = "Blue Team Member 2";	Blue team member 2 name.
 * public const string BlueTeamMember3 = "Blue Team Member 3";	Blue team member 3 name.
 * public const string RedTeamMember1 = "Red Team Member 1";	Red team member 1 name.
 * public const string RedTeamMember2 = "Red Team Member 2";	Red team member 2 name.
 * public const string RedTeamMember3 = "Red Team Member 3";	Red team member 3 name.
 * 
 * _agentData properties and public variables
 * public bool IsPoweredUp;	Have we powered up, true if we’re powered up, false otherwise.
 * public int CurrentHitPoints;	Our current hit points as an integer
 * public bool HasFriendlyFlag;	True if we have collected our own flag
 * public bool HasEnemyFlag;	True if we have collected the enemy flag
 * public GameObject FriendlyBase; The friendly base GameObject
 * public GameObject EnemyBase;    The enemy base GameObject
 * public int FriendlyScore; The friendly teams score
 * public int EnemyScore;       The enemy teams score
 * 
 * _agentActions methods
 * public bool MoveTo(GameObject target)	Move towards a target object. Takes a GameObject representing the target object as a parameter. Returns true if the location is on the navmesh, false otherwise.
 * public bool MoveTo(Vector3 target)	Move towards a target location. Takes a Vector3 representing the destination as a parameter. Returns true if the location is on the navmesh, false otherwise.
 * public bool MoveToRandomLocation()	Move to a random location on the level, returns true if the location is on the navmesh, false otherwise.
 * public void CollectItem(GameObject item)	Pick up an item from the level which is within reach of the agent and put it in the inventory. Takes a GameObject representing the item as a parameter.
 * public void DropItem(GameObject item)	Drop an inventory item or the flag at the agents’ location. Takes a GameObject representing the item as a parameter.
 * public void UseItem(GameObject item)	Use an item in the inventory (currently only health kit or power up). Takes a GameObject representing the item to use as a parameter.
 * public void AttackEnemy(GameObject enemy)	Attack the enemy if they are close enough. ). Takes a GameObject representing the enemy as a parameter.
 * public void Flee(GameObject enemy)	Move in the opposite direction to the enemy. Takes a GameObject representing the enemy as a parameter.
 * 
 * _agentSenses properties and methods
 * public List<GameObject> GetObjectsInViewByTag(string tag)	Return a list of objects with the same tag. Takes a string representing the Unity tag. Returns null if no objects with the specified tag are in view.
 * public GameObject GetObjectInViewByName(string name)	Returns a specific GameObject by name in view range. Takes a string representing the objects Unity name as a parameter. Returns null if named object is not in view.
 * public List<GameObject> GetObjectsInView()	Returns a list of objects within view range. Returns null if no objects are in view.
 * public List<GameObject> GetCollectablesInView()	Returns a list of objects with the tag Collectable within view range. Returns null if no collectable objects are in view.
 * public List<GameObject> GetFriendliesInView()	Returns a list of friendly team AI agents within view range. Returns null if no friendlies are in view.
 * public List<GameObject> GetEnemiesInView()	Returns a list of enemy team AI agents within view range. Returns null if no enemy are in view.
 * public bool IsItemInReach(GameObject item)	Checks if we are close enough to a specific collectible item to pick it up), returns true if the object is close enough, false otherwise.
 * public bool IsInAttackRange(GameObject target)	Check if we're with attacking range of the target), returns true if the target is in range, false otherwise.
 * 
 * _agentInventory properties and methods
 * public bool AddItem(GameObject item)	Adds an item to the inventory if there's enough room (max capacity is 'Constants.InventorySize'), returns true if the item has been successfully added to the inventory, false otherwise.
 * public GameObject GetItem(string itemName)	Retrieves an item from the inventory as a GameObject, returns null if the item is not in the inventory.
 * public bool HasItem(string itemName)	Checks if an item is stored in the inventory, returns true if the item is in the inventory, false otherwise.
 * 
 * You can use the game objects name to access a GameObject from the sensing system. Thereafter all methods require the GameObject as a parameter.
 * 
*****************************************************************************************************************************/

/// <summary>
/// Implement your AI script here, you can put everything in this file, or better still, break your code up into multiple files
/// and call your script here in the Update method. This class includes all the data members you need to control your AI agent
/// get information about the world, manage the AI inventory and access essential information about your AI.
///
/// You may use any AI algorithm you like, but please try to write your code properly and professionaly and don't use code obtained from
/// other sources, including the labs.
///
/// See the assessment brief for more details
/// </summary>
/// 



public class AI : MonoBehaviour
{
    enum NodeOptions { Nothing, RandomMovement, HealthKit, PowerUp, haveEnemyFlag, dontHaveEnemyFlag, FriendlyFlag, FriendlyBase };
    // Gives access to important data about the AI agent (see above)
    private AgentData _agentData;
    // Gives access to the agent senses
    private Sensing _agentSenses;
    // gives access to the agents inventory
    private InventoryController _agentInventory;
    // This is the script containing the AI agents actions
    // e.g. agentScript.MoveTo(enemy);
    private AgentActions _agentActions;
    private MonteCarloTree.Tree MCTree = new MonteCarloTree.Tree();
    private MonteCarloTree.Node rootNode = new MonteCarloTree.Node();
    private MonteCarloTree.Node currentNode;
    private NodeOptions nodeOptions = NodeOptions.Nothing;
    private Vector3 startingPostion;
    // Use this for initialization
    void Start()
    {
        // Initialise the accessable script components
        _agentData = GetComponent<AgentData>();
        _agentActions = GetComponent<AgentActions>();
        _agentSenses = GetComponentInChildren<Sensing>();
        _agentInventory = GetComponentInChildren<InventoryController>();
        currentNode = rootNode;
        rootNode.VisitCount = 1;
        startingPostion = transform.position;
        // nodeOptions = NodeOptions.Nothing;
    }

    // Update is called once per frame
    void Update()
    {
        if (_agentActions.IsAtDestination())
        {
            int option = ChooseRandomAction();

            if (!currentNode.checkChildren(option))
            {
                MonteCarloTree.Node nodeToAdd = new MonteCarloTree.Node();
                nodeToAdd.NodeOption = option;
                currentNode.AddChild(nodeToAdd);

                MonteCarloTree.Node highestUCBNode;
                float highestUCB = 0.0f;
                for (int i = 0; i < currentNode.GetNumberOfChildren(); i++)
                {
                    float UCB = UpperConfidenceBound(currentNode.GetChild(i));
                    if (highestUCB < UCB)
                    {
                        highestUCBNode = currentNode.GetChild(i);
                    }
                }
                currentNode = nodeToAdd;
                currentNode.VisitCount = currentNode.VisitCount + 1;
            }
        }
        if (this.tag == Tags.BlueTeam)
        {
            GameObject ObjectToCheck = _agentSenses.GetObjectInViewByName(Names.RedFlag);

            if (ObjectToCheck != null)
            {
                // _agentActions.MoveTo(ObjectToCheck);
                if (_agentSenses.IsItemInReach(ObjectToCheck))
                {
                    _agentActions.CollectItem(ObjectToCheck);
                }
            }

            if (_agentData.HasEnemyFlag)
            {

                if (Vector3.Distance(transform.position, _agentData.FriendlyBase.transform.position) <= 5.0f)
                {
                    _agentActions.DropItem(_agentInventory.GetItem(Names.RedFlag), new Vector3(0.5f, 1.0f, 21.4f));
                    MonteCarloTree.Node parentNode = currentNode.ParentNode;

                    do
                    {
                        currentNode.WinCount = currentNode.WinCount + 1;
                        currentNode = parentNode;
                    } while (parentNode != null);
                    currentNode = rootNode;
                    currentNode.VisitCount = currentNode.VisitCount + 1;
                    ResetPosition();
                    Debug.Log("win");
                    MCTree.Traverse(rootNode);
                }


            }
        }

        if (Input.GetKeyDown("e"))
        {
            MCTree.Traverse(rootNode);
        }
    }

    private float UpperConfidenceBound(MonteCarloTree.Node node)
    {
        float UCB = (node.WinCount / node.VisitCount) + (0.5f * Mathf.Sqrt(Mathf.Log(node.ParentNode.VisitCount) / node.VisitCount));
        return UCB;
    }

    public int ChooseRandomAction()
    {
        int chosenAction = 0;
        do
        {
            List<GameObject> objectsInView = _agentSenses.GetObjectsInView();
            int action = Random.Range(0, 5) + 1;
            switch (action)
            {
                case 1:
                    // Do we have the enemy flag
                    if (!_agentData.HasEnemyFlag)
                    {
                        if (this.tag == Tags.BlueTeam)
                        {
                            _agentActions.MoveTo(GameObject.Find(Names.RedFlag).transform.position);
                        }
                        else if (this.tag == Tags.RedTeam)
                        {
                            _agentActions.MoveTo(GameObject.Find(Names.BlueFlag).transform.position);
                        }
                        chosenAction = 1;
                    }
                    break;

                case 2:
                    if (_agentData.HasEnemyFlag)
                    {
                        _agentActions.MoveTo(_agentData.FriendlyBase);
                        chosenAction = 2;
                    }
                    break;

                case 3:
                    for (int i = 0; i < objectsInView.Count; i++)
                    {
                        if (objectsInView[i].name == Names.HealthKit)
                        {
                            if (!_agentInventory.HasItem(Names.HealthKit))
                            {
                                _agentActions.MoveTo(objectsInView[i]);
                                chosenAction = 3;
                            }
                        }
                    }
                    break;

                case 4:
                    for (int i = 0; i < objectsInView.Count; i++)
                    {
                        if (objectsInView[i].name == Names.PowerUp)
                        {
                            if (!_agentInventory.HasItem(Names.PowerUp))
                            {
                                _agentActions.MoveTo(objectsInView[i]);
                                chosenAction = 4;
                            }
                        }
                    }
                    break;

                case 5:
                    _agentActions.MoveToRandomLocation();
                    chosenAction = 5;
                    break;

                default:
                    break;
            }

        } while (chosenAction == 0);
        return chosenAction;
    }

    public void ResetPosition()
    {
        foreach (GameObject item in GameObject.FindGameObjectsWithTag(Tags.BlueTeam))
        {
            item.transform.position = item.GetComponent<AI>().startpostion();
        }
        foreach (GameObject item in GameObject.FindGameObjectsWithTag(Tags.RedTeam))
        {
            item.transform.position = item.GetComponent<AI>().startpostion();
        }
    }

    public Vector3 startpostion()
    {
        if (_agentData.HasEnemyFlag)
        {
            if (this.tag == Tags.BlueTeam)
            {
                _agentActions.DropItem(_agentInventory.GetItem(Names.RedFlag), new Vector3(0.5f, 1.0f, 21.4f));
            }
            if (this.tag == Tags.RedTeam)
            {
                _agentActions.DropItem(_agentInventory.GetItem(Names.BlueFlag), new Vector3(0.18f, 1.0f, -22.11f));
            }
        }

        if(_agentData.HasFriendlyFlag)
        {
            if (this.tag == Tags.BlueTeam)
            {
                _agentActions.DropItem(_agentInventory.GetItem(Names.BlueFlag), new Vector3(0.18f, 1.0f, -22.11f));
            }
            if (this.tag == Tags.RedTeam)
            {
                _agentActions.DropItem(_agentInventory.GetItem(Names.RedFlag), new Vector3(0.5f, 1.0f, 21.4f));
            }
        }

        if(_agentInventory.HasItem(Names.HealthKit))
        {
            _agentActions.DropItem(_agentInventory.GetItem(Names.HealthKit));
        }

        if (_agentInventory.HasItem(Names.PowerUp))
        {
            _agentActions.DropItem(_agentInventory.GetItem(Names.PowerUp));
        }
        return startingPostion; 
    }

}