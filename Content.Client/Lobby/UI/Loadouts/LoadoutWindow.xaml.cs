using Content.Client.UserInterface.Controls;
using Content.Shared._NF.Bank;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.Lobby.UI.Loadouts;

[GenerateTypedNameReferences]
public sealed partial class LoadoutWindow : FancyWindow
{
    public event Action<ProtoId<LoadoutGroupPrototype>, ProtoId<LoadoutPrototype>>? OnLoadoutPressed;
    public event Action<ProtoId<LoadoutGroupPrototype>, ProtoId<LoadoutPrototype>>? OnLoadoutUnpressed;

    private List<LoadoutGroupContainer> _groups = new();

    public HumanoidCharacterProfile Profile;

    public LoadoutWindow(HumanoidCharacterProfile profile, RoleLoadout loadout, RoleLoadoutPrototype proto, ICommonSession session, IDependencyCollection collection)
    {
        RobustXamlLoader.Load(this);
        Profile = profile;
        var protoManager = collection.Resolve<IPrototypeManager>();

        foreach (var group in proto.Groups)
        {
            if (!protoManager.TryIndex(group, out var groupProto))
                continue;

            if (groupProto.Hidden)
                continue;

            var container = new LoadoutGroupContainer(profile, loadout, protoManager.Index(group), session, collection);
            LoadoutGroupsContainer.AddTab(container, Loc.GetString(groupProto.Name));
            _groups.Add(container);
            container.OnLoadoutPressed += args =>
            {
                OnLoadoutPressed?.Invoke(group, args);
            };

            container.OnLoadoutUnpressed += args =>
            {
                OnLoadoutUnpressed?.Invoke(group, args);
            };
        }
        //Frontier - we inject our label here but it needs recalculating every time a new item is selected,
        //so we add a new method and call it there too.
        CalculateLoadoutCost(loadout, collection);
        // Frontier - update bank balance label text - value should not change.
        Balance.Margin = new Thickness(5, 2, 5, 5);
        Balance.Text = Loc.GetString("frontier-loadout-balance", ("balance", BankSystemExtensions.ToSpesoString(Profile.BankBalance)));
    }

    public void RefreshLoadouts(RoleLoadout loadout, ICommonSession session, IDependencyCollection collection)
    {
        foreach (var group in _groups)
        {
            group.RefreshLoadouts(Profile, loadout, session, collection);
        }

        CalculateLoadoutCost(loadout, collection); //Frontier
    }

    /// <summary>
    /// Frontier function to calculate and update the label.
    /// </summary>
    /// <param name="loadout">The currently selected loadout</param>
    /// <param name="collection">IDependency Collection of various dependencies</param>
    private void CalculateLoadoutCost(RoleLoadout loadout, IDependencyCollection collection)
    {
        var protoManager = collection.Resolve<IPrototypeManager>();
        var cost = 0;
        foreach (var loadoutGroup in loadout.SelectedLoadouts)
        {
            foreach (var equipment in loadoutGroup.Value)
            {
                if (protoManager.TryIndex(equipment.Prototype, out var equipProto))
                {
                    cost += equipProto.Price;
                }
            }
        }

        Cost.Margin = new Thickness(5, 2, 5, 5);
        Cost.Text = Loc.GetString("frontier-loadout-cost", ("cost", BankSystemExtensions.ToSpesoString(cost)));
    }
    // End Frontier
}
