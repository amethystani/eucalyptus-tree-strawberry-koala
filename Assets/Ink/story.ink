// ═══════════════════════════════════════════════════════════════════════════
// THE EUCALYPTUS TREE & THE STRAWBERRY KOALA
// ═══════════════════════════════════════════════════════════════════════════

VAR goofy = 0
VAR overthinker = 0
VAR lives = 3
VAR has_strawberry = false
VAR has_keychain = false
VAR momo_done = false
VAR dietcoke_done = false

=== prologue ===
#scene:dorm #sfx:music_lofi
In a universe that felt completely black, a Monkey found his constellation.
-> dorm_room


// ═══════════════════════════════════════════════════════════════════════════
// PROLOGUE — DORM ROOM  (4 AM)
// ═══════════════════════════════════════════════════════════════════════════
=== dorm_room ===
#mood:eepy
It is 4 AM. The overthinking hour.
You reach into your bag — for the third time.
The Crochet Strawberry. The one she made. Gone.
+ [Check bag again] #overthinker:+10
    -> check_bag_again
+ [Just go find it. Now.] #goofy:+5
    -> leave_for_cnd

=== check_bag_again ===
Nope. Definitely not there.
Your brain starts running scenarios.
What if you lost it at the library? What if someone took it? What if—
+ [Stop. Go to CND.] #overthinker:+5
    -> leave_for_cnd
+ [Check one more time] #overthinker:+10
    -> check_bag_final

=== check_bag_final ===
Still not there. Obviously. It was never going to be there.
-> leave_for_cnd

=== leave_for_cnd ===
#scene:worldmap
The world map opens. CND is calling.
-> cnd_arrive


// ═══════════════════════════════════════════════════════════════════════════
// ACT I — CND CAFE
// ═══════════════════════════════════════════════════════════════════════════
=== cnd_arrive ===
#scene:cnd #sfx:music_cnd #mood:neutral
CND. The smell of momos and bad decisions.
#npc:dhruv
Dhruv is there, eating like he has no worries in the world.
#mood:neutral
Dhruv: Aye, you look like a ghost. What happened?
+ ["Miya miya"] #goofy:+10
    -> dhruv_miya
+ ["I'm just bedrotting"] #overthinker:+10
    -> dhruv_bedrot
+ ["My strawberry is missing"] #overthinker:+5
    -> dhruv_strawberry_direct

=== dhruv_miya ===
#mood:talking
Dhruv: (laughs) Okay okay. Sit. What's actually wrong?
-> dhruv_quest_start

=== dhruv_bedrot ===
#mood:neutral
Dhruv: You and me both, bhai. You and me both.
Dhruv: Also you look like you lost something.
-> dhruv_quest_start

=== dhruv_strawberry_direct ===
#mood:talking
Dhruv: The crochet one? From HER? Bro.
Dhruv: I saw you had it at the library yesterday. Third row, IR section.
#goofy:+5
-> dhruv_quest_start

=== dhruv_quest_start ===
#npc:nischala
Nischala materialises from nowhere, as she does.
#mood:neutral
Nischala: You need to find something. I can feel it.
#mood:talking
Dhruv: Before I help — I'm starving. Momo Sizzler. Go.
Nischala: Diet Coke. And I'll tell you something useful.
+ [Fine. I'll get both.] #goofy:+5
    -> fetch_quest
+ [Are you serious right now?] #overthinker:+10
    -> fetch_annoyed

=== fetch_annoyed ===
#npc:dhruv
Dhruv: Dead serious. Sizzler first.
-> fetch_quest

=== fetch_quest ===
#scene:cnd
You order at the counter.
The aunty gives you a look that means everything and nothing.
~ momo_done = true
~ dietcoke_done = true
-> fetch_delivered

=== fetch_delivered ===
#npc:dhruv #mood:happy
Dhruv: (already eating) YESSS. Okay okay. Library. Third row. IR section.
The strawberry is probably stuck in a book. You know how you are.
#npc:nischala #mood:talking
Nischala: And Monkey — she's been there since 11 AM.
Nischala: She looks eepy but she's waiting.
#goofy:+10
-> unlock_library

=== unlock_library ===
#scene:worldmap
Library unlocked.
-> library_stealth_start


// ═══════════════════════════════════════════════════════════════════════════
// ACT II — LIBRARY STEALTH
// ═══════════════════════════════════════════════════════════════════════════
=== library_stealth_start ===
#scene:library #sfx:music_library #mood:neutral
The library. Fluorescent lights. That specific silence.
Prof. Jabin is patrolling the IR section. Third row.
You need to reach it without being seen.
#stealth:begin
* [stealth_success]
    -> found_strawberry
* [stealth_caught_once]
    -> caught_once
* [stealth_caught_twice]
    -> caught_twice
* [stealth_bad_detour]
    -> bad_detour

=== caught_once ===
#npc:jabin #mood:angry
Prof. Jabin: You there! This is a study zone, not a playground.
Prof. Jabin: Extra reading. Forty pages of Waltz. By Friday.
#overthinker:+15
lives = lives - 1
You slip away. Two chances left. Stay low.
-> library_stealth_start

=== caught_twice ===
#npc:jabin #mood:angry
Prof. Jabin: AGAIN? Are you incapable of reading the room?
Prof. Jabin: Sixty pages. AND a reflection essay on the concept of stealth.
#overthinker:+20
lives = lives - 1
One last chance.
-> library_stealth_start

=== bad_detour ===
#scene:dorm #mood:eepy #lives:-1
Jabin confiscated your library card.
By the time you sorted it out, it was 7 PM.
You never made it.
Your phone buzzes.
Slushy: "I hate u, dekh le."
#ending:grey
-> END

=== found_strawberry ===
#mood:happy #goofy:+15
~ has_strawberry = true
There it is.
Wedged between "Theory of International Politics" and "World Order."
Of course it was in IR.
You hold it for exactly two seconds.
Then you run for the Metro.
-> unlock_metro


// ═══════════════════════════════════════════════════════════════════════════
// ACT III — METRO + ART GALLERY
// ═══════════════════════════════════════════════════════════════════════════
=== unlock_metro ===
#scene:worldmap
Metro unlocked.
-> metro_arrive

=== metro_arrive ===
#scene:metro #sfx:rain #sfx:metro #mood:neutral
Rajiv Chowk. It is raining. The platform is absolutely packed.
{ goofy > 35:
    Something shifts.
    Your walk changes. Your whole energy changes.
    #mood:princess-ani #goofy:+10
    Princess Ani mode: activated.
    Baggy jeans. Unbothered stride. Let's go.
}
The crowd is thick. There's no clean path.
+ [Excuse me, excuse me...] #overthinker:+5
    -> metro_gentle
+ [Just vibe through it] #goofy:+10
    -> metro_vibe

=== metro_gentle ===
You make it through. Somehow. Without eye contact.
-> gallery_arrive

=== metro_vibe ===
#mood:goofy
Three aunties smile at you. One uncle nods.
You don't know why. You don't question it.
#goofy:+5
-> gallery_arrive

=== gallery_arrive ===
#scene:gallery #sfx:music_gallery
The Art Gallery. Warm light. Quiet hum of people pretending to understand art.
#npc:slushy
#mood:eepy
She's there. Standing in front of a painting of the sea.
She doesn't turn around but she knew.
Slushy: You're late.
+ [Hand her the strawberry] { has_strawberry: -> give_strawberry | -> no_strawberry }
+ ["I can explain—"] #overthinker:+10
    -> explain_late

=== explain_late ===
#mood:straight
Slushy: (long pause) Just... come here.
-> gallery_moment

=== no_strawberry ===
#overthinker:+20
Monkey: I... forgot it.
#mood:straight
Slushy: Of course you did.
-> gallery_moment

=== give_strawberry ===
#mood:happy
Her face does the thing.
Slushy: You went back for it?
Monkey: I never lost it. I always knew where it was.
(That was a lie. But a good one.)
{ goofy > overthinker:
    #goofy:+10
    You reach into your other pocket.
    The lily of the valley keychain.
    You were going to give it to her anyway.
    ~ has_keychain = true
    Slushy: (quietly) Oh.
}
-> gallery_moment

=== gallery_moment ===
#mood:neutral
She turns back to the painting.
Slushy: You know what you are?
+ [What?] -> ending_check
+ [Your problem?] #goofy:+10 -> your_problem

=== your_problem ===
#mood:happy
Slushy: (doesn't deny it)
-> ending_check


// ═══════════════════════════════════════════════════════════════════════════
// ENDINGS
// ═══════════════════════════════════════════════════════════════════════════
=== ending_check ===
{ has_keychain && goofy > overthinker:
    -> ending_constellation
- goofy > overthinker:
    -> ending_constellation
- goofy == overthinker:
    -> ending_milkshake
- else:
    -> ending_grey_normal
}

=== ending_constellation ===
#ending:constellation #mood:happy #sfx:music_stars
The gallery lights dim a little. The window behind her shows the city.
She leans back against the wall.
Slushy: You're my eucalyptus tree.
Monkey: What does that mean?
Slushy: It means you're tall and you smell a little weird and I like you.
You stay until closing time. The security guard has to ask twice.
-> END

=== ending_milkshake ===
#ending:milkshake #mood:goofy #sfx:music_bole
You forgot the gift.
But your legs started moving before you could think about it.
Bole Chudiyan. Full choreo. In the Art Gallery.
Two people recorded it.
#mood:straight
Your phone buzzes.
Slushy: 😐
Slushy: You're so embarrassing
Slushy: I'm telling you this as a friend
Three dots.
Slushy: ...come outside
-> END

=== ending_grey_normal ===
#ending:grey #mood:eepy #sfx:music_rain
You overthought the whole walk there.
You overthought what to say.
You said nothing.
She figured it out before you did. She always does.
The train home is quiet.
Your phone buzzes.
Slushy: "I hate u, dekh le."
You stare at it for a long time.
You start typing. Then stop. Then start again.
-> END
