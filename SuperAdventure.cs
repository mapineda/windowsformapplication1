using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;

namespace WindowsFormsApplication1
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        public SuperAdventure()
        {
            InitializeComponent();

            Location location = new Location(1, "Home", "This is your house.");

            _player = new Player(10, 10, 20, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void SuperAdventure_Load(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            //does the location have any required itmes
            if (newLocation.ItemRequiredToEnter != null)
            {
                //see if the player has the required item in their inventory
                bool playerHasRequiredItem = false;

                foreach (InventoryItem ii in _player.Inventory)
                {
                    if (ii.Details.ID == newLocation.ItemRequiredToEnter.ID)
                    {
                        //We found the required item
                        playerHasRequiredItem = true;
                        break; //exit out of the foreach loop
                    }
                }

                if (!playerHasRequiredItem)
                {
                    //we didn't find the required item in the inventory - display a message and stop trying to move
                    rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                    return;
                }
            }

            //update the player's current location
            _player.CurrentLocation = newLocation;

            //show/hide available movement buttons
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnNorth.Visible = (newLocation.LocationToNorth != null);

            //Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //Completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            //update hit points in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            //Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                //see if player already has the quest and if they have compeleted it
                bool playerAlreadyHasQuest = false;
                bool playerAlreadyCompletedQuest = false;

                foreach (PlayerQuest playerQuest in _player.Quests)
                {
                    if (playerQuest.Details.ID == newLocation.QuestAvailableHere.ID)
                    {
                        playerAlreadyHasQuest = true;

                        if (playerQuest.IsCompleted)
                        {
                            playerAlreadyCompletedQuest = true;
                        }
                    }
                }

                //see if the player already has the quest
                if (playerAlreadyHasQuest)
                {
                    //if the player has not completed the quest yet
                    if (!playerAlreadyCompletedQuest)
                    {
                        //see if the player has all the items needed to complete the quest
                        bool playerHasAllItemsToCompleteQuest = true;

                        foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                        {
                            bool foundItemInPlayersInventory = false;
                            //check each item in the player's inventory, to see if they have it, and enough of it
                            foreach (InventoryItem ii in _player.Inventory)
                            {
                                //the player has this item in their inventory
                                if (ii.Details.ID == qci.Details.ID)
                                {
                                    foundItemInPlayersInventory = true;

                                    if (ii.Quantity < qci.Quantity)
                                    {
                                        //the player does not have enough of this item to complete the quest
                                        playerHasAllItemsToCompleteQuest = false;

                                        //ther is no reason to continue checking for the other quest completion items
                                        break;
                                    }
                                    //we found the item, so don't check the rest of the player's inventory
                                    break;
                                }
                            }
                            //if we didn't find the required items, set our variable and stop looking for other items
                            if (!foundItemInPlayersInventory)
                            {
                                //the player does not ahve this item in their inventory
                                playerHasAllItemsToCompleteQuest = false;

                                break;
                            }
                        }
                        //the player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;

                            //remove quest items from inventory
                            foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                            {
                                foreach (InventoryItem ii in _player.Inventory)
                                {
                                    if (ii.Details.ID == qci.Details.ID)
                                    {
                                        //subtrack the quantity from the player's inventory that was needed to complete the quest
                                        ii.Quantity -= qci.Quantity;
                                        break;
                                    }
                                }
                            }
                            //Give Quest rewards
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " experience points" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " gold" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;

                            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            //add the reward item to the players inventory
                            bool addedItemToPlayerInventory = false;

                            foreach (InventoryItem ii in _player.Inventory)
                            {
                                if (ii.Details.ID == newLocation.QuestAvailableHere.RewardItem.ID)
                                {
                                    //they have the item in their inventory so increas the quantity by one
                                    ii.Quantity++;
                                    addedItemToPlayerInventory = true;
                                    break;
                                }
                            }
                            //they didnt have the item, so add it to their inventory, with a quanity of 1
                            if (!addedItemToPlayerInventory)
                            {
                                _player.Inventory.Add(new InventoryItem(newLocation.QuestAvailableHere.RewardItem, 1));
                                //mark the quest as completed
                                //find the quest in the player's quest list
                                foreach (PlayerQuest pq in _player.Quests)
                                {
                                    if (pq.Details.ID == newLocation.QuestAvailableHere.ID)
                                    {
                                        //mark it as completed
                                        pq.IsCompleted = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //the player does not already have the quest
                        //Display the messages
                        rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                        rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                        rtbMessages.Text += "To complete it, return with: " + Environment.NewLine;
                        foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                        {
                            if (qci.Quantity == 1)
                            {
                                rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                            }
                        }
                        rtbMessages.Text += Environment.NewLine;
                        //add the quest to the player's quest list
                        _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                    }
                }
                //does the location have a monster?
                if (newLocation.MonsterLivingHere != null)
                {
                    rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                    //make a new monster, using the values from the standard monster in the World.Monster list
                    Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                    _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                    foreach (LootItem lootitem in standardMonster.LootTable)
                    {
                        _currentMonster.LootTable.Add(lootitem);
                    }

                    cboWeapons.Visible = true;
                    cboPotions.Visible = true;
                    btnUseWeapon.Visible = true;
                    btnUsePotion.Visible = true;
                }
                else
                {
                    _currentMonster = null;

                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;
                }

                //refresh player's inventory list
                dgvInventory.RowHeadersVisible = false;

                dgvInventory.ColumnCount = 2;
                dgvInventory.Columns[0].Name = "Name";
                dgvInventory.Columns[0].Width = 197;
                dgvInventory.Columns[1].Name = "Quantity";

                dgvInventory.Rows.Clear();

                foreach (InventoryItem inventoryItem in _player.Inventory)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                    }
                }
                //refresh player's quest list
                dgvQuests.RowHeadersVisible = false;

                dgvQuests.ColumnCount = 2;
                dgvQuests.Columns[0].Name = "Name";
                dgvQuests.Columns[0].Width = 197;
                dgvQuests.Columns[1].Name = "Done?";

                dgvQuests.Rows.Clear();

                foreach (PlayerQuest playerQuest in _player.Quests)
                {
                    dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
                }

                //refresh player's weapons combobox
                List<Weapon> weapons = new List<Weapon>();

                foreach (InventoryItem inventoryItem in _player.Inventory)
                {
                    if (inventoryItem.Details is Weapon)
                    {
                        if (inventoryItem.Quantity > 0)
                        {
                            weapons.Add((Weapon)inventoryItem.Details);
                        }
                    }
                }
                if (weapons.Count == 0)
                {
                    //the player doesnt have any weapons, so hide the weapon combo box and the "use" button
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
                else
                {
                    cboWeapons.DataSource = weapons;
                    cboWeapons.DisplayMember = "Name";
                    cboWeapons.ValueMember = "ID";

                    cboWeapons.SelectedIndex = 0;
                }
                //refresh the player's potion combobox
                List<HealingPotion> healingPotions = new List<HealingPotion>();

                foreach (InventoryItem inventoryItem in _player.Inventory)
                {
                    if (inventoryItem.Details is HealingPotion)
                    {
                        if (inventoryItem.Quantity > 0)
                        {
                            healingPotions.Add((HealingPotion)inventoryItem.Details);
                        }
                    }
                }
                if (healingPotions.Count == 0)
                {
                    //the player doesnt have any potions, so hide the potion combobox and the "use" button
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
                else
                {
                    cboPotions.DataSource = healingPotions;
                    cboPotions.DisplayMember = "Name";
                    cboPotions.ValueMember = "ID";
                    cboPotions.SelectedIndex = 0;
                }
            }
            private void btnUseWeapon_Click(object sender, EventArgs e)
            {
            }

            private void btnUsePotion_Click(object sender, EventArgs e)
            {
            }
        }
    }
