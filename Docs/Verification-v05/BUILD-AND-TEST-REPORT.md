# Pocket Mech Arena v0.5 verification

Unity 6000.6.0f1 built the Windows player and Web player successfully with zero C# errors. The final Web compiler reported editor-only deprecation warnings for Unity's older object-search overload; these do not execute in the game.

The Windows player passed all 54 checks in `smoke-results.txt`, including two complete assisted five-minute runs, original and new bosses, new machine attacks, area switching, difficulty rewards, legacy save compatibility and existing combat/equipment regressions. Tests restore hull and force any surviving boss defeat at 04:58; they do not measure human difficulty.

The PNG files are actual Unity camera/UI captures. `09-frostline-select.png` shows the new selector and `10-frostline-combat.png` shows the new area and all four new machines. Initial visual inspection caught a duplicate old background and incorrect atlas source scaling. Both were corrected, rebuilt and recaptured before publication.

The local Web build loads through the browser landing screen and displays both mission options and Frostline's correct preview/rewards. Selecting an area produces a Unity persistence deprecation warning, but no browser error. Native Safari and physical iPhone tests are still required. Browser download is approximately 21 MB.
