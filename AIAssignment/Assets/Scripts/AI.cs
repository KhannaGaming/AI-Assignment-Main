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
    enum NodeOptions { Nothing, dontHaveEnemyFlag, haveEnemyFlag, HealthKit, PowerUp, RandomMovement, FriendlyFlag };
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
    private bool hasWon = false;
    private float ExplorationFactor = 0.5f;
    private bool isExploring = true;

    private bool inBattle = false;

    private void Awake()
    {
        //if (Application.isEditor)
        Application.runInBackground = true;

    }

    // Use this for initialization
    void Start()
    {
        // Initialise the accessable script components
        _agentData = GetComponent<AgentData>();
        _agentActions = GetComponent<AgentActions>();
        _agentSenses = GetComponentInChildren<Sensing>();
        _agentInventory = GetComponentInChildren<InventoryController>();
        currentNode = rootNode;
        startingPostion = transform.position;
        // nodeOptions = NodeOptions.Nothing;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            Time.timeScale += 1.0f;
            //_agentData.Die();
        }
        if (Input.GetKeyDown("r"))
        {
            Time.timeScale = 1.0f;
        }

        //To Test if Monte Carlo Tree works, switch off exploration
        if (Input.GetKeyDown("t"))
        {
            if (ExplorationFactor == 0.0f)
            {
                ExplorationFactor = 0.5f;
            }
            else
            {
                ExplorationFactor = 0.0f;
            }
            isExploring = !isExploring;
            ResetPosition();
            GameObject.Find("IsExploring").GetComponent<UnityEngine.UI.Text>().text = isExploring.ToString();
        }


        if (!inBattle && _agentActions.IsAtDestination())
        {
            SelectionAndExpantion();
            Simulation((int)nodeOptions);
        }

        Battle();
        Checks();

    }

    private float UpperConfidenceBound(MonteCarloTree.Node node)
    {
        float UCB = 0.0f;

        if (node.VisitCount > 0)
        {
            UCB = (node.WinCount / node.VisitCount) + (ExplorationFactor * Mathf.Sqrt(Mathf.Log(node.ParentNode.VisitCount) / node.VisitCount));
        }
        else if (isExploring)
        {
            UCB = int.MaxValue;
        }
        return UCB;
    }

    public int ChooseRandomAction()
    {
        int chosenAction = 0;
        do
        {
            List<GameObject> objectsInView = _agentSenses.GetObjectsInView();
            int action = Random.Range(0, 6) + 1;
            switch (action)
            {
                case 1:
                    // Do we have the enemy flag
                    if (!_agentData.HasEnemyFlag)
                    {
                        chosenAction = 1;
                    }
                    break;

                case 2:
                    if (_agentData.HasEnemyFlag)
                    {
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
                                chosenAction = 4;
                            }
                        }
                    }
                    break;

                case 5:
                    chosenAction = 5;
                    break;
                case 6:
                    for (int i = 0; i < objectsInView.Count; i++)
                    {
                        if (objectsInView[i].name == Names.BlueFlag)
                        {
                            if (this.tag == Tags.BlueTeam)
                            {
                                chosenAction = 6;
                            }
                        }
                        else if (objectsInView[i].name == Names.RedFlag)
                        {
                            if (this.tag == Tags.RedTeam)
                            {
                                chosenAction = 6;
                            }
                        }
                    }
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
            item.transform.position = item.GetComponent<AI>().startPostion();
            item.GetComponent<AI>().ResetHealth();
        }
        foreach (GameObject item in GameObject.FindGameObjectsWithTag(Tags.RedTeam))
        {
            item.transform.position = item.GetComponent<AI>().startPostion();
            item.GetComponent<AI>().ResetHealth();
        }
    }

    public Vector3 startPostion()
    {
        if (_agentData.HasEnemyFlag)
        {
            if (this.tag == Tags.BlueTeam)
            {
                _agentInventory.RemoveItem(Names.RedFlag);
                GameObject.Find(Names.RedFlag).GetComponent<Flag>().ResetPositionRed();
            }
            if (this.tag == Tags.RedTeam)
            {
                _agentInventory.RemoveItem(Names.BlueFlag);
                GameObject.Find(Names.BlueFlag).GetComponent<Flag>().ResetPositionBlue();
            }
        }

        if (_agentData.HasFriendlyFlag)
        {
            if (this.tag == Tags.BlueTeam)
            {
                GameObject.Find(Names.BlueFlag).GetComponent<Flag>().ResetPositionBlue();
            }
            if (this.tag == Tags.RedTeam)
            {
                GameObject.Find(Names.RedFlag).GetComponent<Flag>().ResetPositionRed();
            }
        }

        if (_agentInventory.HasItem(Names.HealthKit))
        {
            _agentActions.DropItem(_agentInventory.GetItem(Names.HealthKit));
            GameObject.Find(Names.HealthKit).GetComponent<Collectable>().resetPosition();
        }

        if (_agentInventory.HasItem(Names.PowerUp))
        {
            _agentActions.DropItem(_agentInventory.GetItem(Names.PowerUp));
            GameObject.Find(Names.PowerUp).GetComponent<Collectable>().resetPosition();
        }

        if (hasWon)
        {
            BackpropagationIfWon();
        }
        else
        {
            BackpropagationIfLost();
        }

        return startingPostion;
    }

    public void ResetHealth()
    {
        _agentData.Heal(100);
    }

    private void SelectionAndExpantion()
    {
        int option = ChooseRandomAction();

        if (!currentNode.checkChildren(option))
        {
            MonteCarloTree.Node nodeToAdd = new MonteCarloTree.Node();
            nodeToAdd.NodeOption = option;
            currentNode.AddChild(nodeToAdd);
        }

        float highestUCB = -1.0f;
        MonteCarloTree.Node testNode = currentNode;
        for (int i = 0; i < currentNode.GetNumberOfChildren(); i++)
        {
            float UCB = UpperConfidenceBound(currentNode.GetChild(i));
            if (highestUCB < UCB)
            {
                testNode = currentNode.GetChild(i);
                highestUCB = UCB;
            }
        }
        currentNode = testNode;
        nodeOptions = (NodeOptions)currentNode.NodeOption;
    }

    private void Simulation(int option)
    {
        switch (option)
        {

            case 1:
                if (this.tag == Tags.BlueTeam)
                {
                    _agentActions.MoveTo(GameObject.Find(Names.RedFlag).transform.position);
                }
                else if (this.tag == Tags.RedTeam)
                {
                    _agentActions.MoveTo(GameObject.Find(Names.BlueFlag).transform.position);
                }
                break;

            case 2:
                _agentActions.MoveTo(_agentData.FriendlyBase);
                break;

            case 3:
                _agentActions.MoveTo(_agentSenses.GetObjectInViewByName(Names.HealthKit));
                break;

            case 4:
                _agentActions.MoveTo(_agentSenses.GetObjectInViewByName(Names.PowerUp));
                break;

            case 5:
                _agentActions.MoveToRandomLocation();
                break;

            case 6:
                if (this.tag == Tags.BlueTeam)
                {
                    _agentActions.MoveTo(_agentSenses.GetObjectInViewByName(Names.BlueFlag));
                }
                else if (this.tag == Tags.RedTeam)
                {
                    _agentActions.MoveTo(_agentSenses.GetObjectInViewByName(Names.RedFlag));
                }
                break;

            default:
                break;
        }
    }

    private void BackpropagationIfLost()
    {
        do
        {
            currentNode.VisitCount = currentNode.VisitCount + 1;
            currentNode = currentNode.ParentNode;
        } while (currentNode != null);
        currentNode = rootNode;
    }

    private void BackpropagationIfWon()
    {
        do
        {
            currentNode.WinCount = currentNode.WinCount + 1;
            currentNode.VisitCount = currentNode.VisitCount + 1;
            currentNode = currentNode.ParentNode;
        } while (currentNode != null);
        currentNode = rootNode;
    }

    private void Checks()
    {
        if (_agentInventory.HasItem(Names.HealthKit))
        {
            if (_agentData.CurrentHitPoints <= 50)
            {
                _agentActions.UseItem(_agentInventory.GetItem(Names.HealthKit));
            }
        }

        GameObject ObjectToCheck = _agentSenses.GetObjectInViewByName(Names.HealthKit);
        if (ObjectToCheck != null)
        {
            if (!_agentInventory.HasItem(Names.HealthKit))
            {
                if (_agentSenses.IsItemInReach(ObjectToCheck))
                {
                    _agentActions.CollectItem(ObjectToCheck);
                    Simulation((int)nodeOptions);
                }
                else if (Vector3.Distance(transform.position, ObjectToCheck.transform.position) < 7.0f)
                {
                    _agentActions.MoveTo(ObjectToCheck);
                }
            }
        }

        ObjectToCheck = _agentSenses.GetObjectInViewByName(Names.PowerUp);
        if (ObjectToCheck != null)
        {
            if (!_agentInventory.HasItem(Names.PowerUp))
            {
                if (_agentSenses.IsItemInReach(ObjectToCheck))
                {
                    _agentActions.CollectItem(ObjectToCheck);
                    Simulation((int)nodeOptions);
                }
                else if (Vector3.Distance(transform.position, ObjectToCheck.transform.position) < 7.0f)
                {
                    _agentActions.MoveTo(ObjectToCheck);
                }
            }
        }

        if (this.tag == Tags.BlueTeam)
        {
            ObjectToCheck = _agentSenses.GetObjectInViewByName(Names.RedFlag);

            if (ObjectToCheck != null)
            {
                if (_agentSenses.IsItemInReach(ObjectToCheck))
                {
                    _agentActions.CollectItem(ObjectToCheck);
                    Simulation((int)nodeOptions);
                }
            }

            ObjectToCheck = _agentSenses.GetObjectInViewByName(Names.BlueFlag);

            if (ObjectToCheck != null)
            {
                if (_agentSenses.IsItemInReach(ObjectToCheck))
                {
                    ObjectToCheck.GetComponent<Flag>().ResetPositionBlue();
                    Simulation((int)nodeOptions);
                }
            }

            if (_agentData.HasEnemyFlag)
            {
                if (Vector3.Distance(transform.position, _agentData.FriendlyBase.transform.position) <= 5.0f)
                {
                    _agentActions.DropItem(_agentInventory.GetItem(Names.RedFlag));
                    hasWon = true;
                    ResetPosition();
                    hasWon = false;
                    currentNode = rootNode;
                    Debug.Log("win" + this.gameObject);
                }
            }
        }

        if (this.tag == Tags.RedTeam)
        {
            ObjectToCheck = _agentSenses.GetObjectInViewByName(Names.BlueFlag);

            if (ObjectToCheck != null)
            {
                if (_agentSenses.IsItemInReach(ObjectToCheck))
                {
                    _agentActions.CollectItem(ObjectToCheck);
                    Simulation((int)nodeOptions);
                }
            }

            ObjectToCheck = _agentSenses.GetObjectInViewByName(Names.RedFlag);

            if (ObjectToCheck != null)
            {
                if (_agentSenses.IsItemInReach(ObjectToCheck))
                {
                    ObjectToCheck.GetComponent<Flag>().ResetPositionRed();
                    Simulation((int)nodeOptions);
                }
            }

            if (_agentData.HasEnemyFlag)
            {

                if (Vector3.Distance(transform.position, _agentData.FriendlyBase.transform.position) <= 5.0f)
                {
                    _agentActions.DropItem(_agentInventory.GetItem(Names.BlueFlag));
                    hasWon = true;
                    ResetPosition();
                    hasWon = false;
                    Debug.Log("win" + this.gameObject);
                }
            }
        }
    }

    private void Battle()
    {
        if (!_agentData.HasEnemyFlag)
        {
            GameObject checkFlag;
            if (_agentSenses.GetEnemiesInView().Count > 0)
            {
                for (int i = 0; i < _agentSenses.GetEnemiesInView().Count; i++)
                {
                    if (_agentData.CurrentHitPoints > 50)
                    {
                        if (_agentSenses.IsInAttackRange(_agentSenses.GetEnemiesInView()[i]))
                        {
                            inBattle = true;
                            _agentActions.UseItem(_agentInventory.GetItem(Names.PowerUp));
                            _agentActions.AttackEnemy(_agentSenses.GetEnemiesInView()[i]);
                        }
                        else if(this.tag == Tags.BlueTeam)
                        {
                            checkFlag = _agentSenses.GetObjectInViewByName(Names.RedFlag);
                            if(checkFlag!= null)
                            {
                                if (Vector3.Distance(transform.position, checkFlag.transform.position) < Vector3.Distance(transform.position, _agentSenses.GetEnemiesInView()[i].transform.position))
                                {
                                    _agentActions.MoveTo(checkFlag);
                                }
                            }
                            else
                            {
                                inBattle = true;
                                _agentActions.MoveTo(_agentSenses.GetEnemiesInView()[i]);
                            }
                        }
                        else if (this.tag == Tags.RedTeam)
                        {
                            checkFlag = _agentSenses.GetObjectInViewByName(Names.BlueFlag);
                            if (checkFlag != null)
                            {
                                if (Vector3.Distance(transform.position, checkFlag.transform.position) < Vector3.Distance(transform.position, _agentSenses.GetEnemiesInView()[i].transform.position))
                                {
                                    _agentActions.MoveTo(checkFlag);
                                }
                            }
                            else
                            {
                                inBattle = true;
                                _agentActions.MoveTo(_agentSenses.GetEnemiesInView()[i]);
                            }
                        }
                    }
                    else if (_agentData.CurrentHitPoints <= 10)
                    {
                        if (_agentData.IsPoweredUp)
                        {
                            inBattle = true;
                            _agentActions.AttackEnemy(_agentSenses.GetEnemiesInView()[i]);
                        }
                        else
                        {
                            int RandomChanceToAttack = Random.Range(0, 100) + 1;
                            if (RandomChanceToAttack > 35)
                            {
                                inBattle = true;
                                _agentActions.AttackEnemy(_agentSenses.GetEnemiesInView()[i]);
                            }
                            else
                            {
                                inBattle = false;
                                _agentActions.Flee(_agentSenses.GetEnemiesInView()[i]);
                            }
                        }
                    }
                    else
                    {
                        if (_agentSenses.IsInAttackRange(_agentSenses.GetEnemiesInView()[i]))
                        {
                            int RandomChanceToAttack = Random.Range(0, 2);
                            if (RandomChanceToAttack == 0)
                            {
                                inBattle = true;
                                _agentActions.UseItem(_agentInventory.GetItem(Names.PowerUp));
                                _agentActions.AttackEnemy(_agentSenses.GetEnemiesInView()[i]);
                            }
                            else
                            {
                                if (_agentData.IsPoweredUp)
                                {
                                    inBattle = true;
                                    _agentActions.AttackEnemy(_agentSenses.GetEnemiesInView()[i]);
                                }
                                else
                                {
                                    int RandomChanceToFlee = Random.Range(0, 2);
                                    if (RandomChanceToFlee == 0)
                                    {
                                        _agentActions.MoveToRandomLocation();
                                    }
                                    else
                                    {
                                    _agentActions.Flee(_agentSenses.GetEnemiesInView()[i]);
                                    }
                                    inBattle = false;
                                }
                            }
                        }
                        else
                        {
                            inBattle = false;
                        }
                    }
                }
            }
            else
            {
                inBattle = false;
            }
        }
        else
        {
            inBattle = false;
        }
    }
}