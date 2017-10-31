using UnityEngine;
using System.Collections;

public class AFInAppEvents {
    /*
    Slotica Event 
    */
    // type Name
    public const string PLAY = "af_play";
    // Event Parameter Name
    public const string ACCOUNT_LOGIN = "af_account_login";
    public const string LOBBY_LOADING = "af_lobby_loading";
    public const string USER_DATA_LOADING = "af_user_data_loading";
    public const string APP_NORMAL_EXIT = "af_app_normal_exit";

    // type Name
    public const string GAME = "af_game";
    // Event Parameter Name
    public const string GAME_SELECT = "af_game_select";
    public const string SPIN_COUNT = "af_spin_count";
    public const string GAME_EXIT = "af_game_exit";

    // type Name
    public const string USER = "af_user";
    // Event Parameter Name
    public const string USER_LEVEL = "af_user_level";
    public const string LEVEL_UP = "af_level_up";
    public const string COINS_BALANCE = "af_coins_balance";

    // type Name
    public const string PURCHASE = "af_purchase";
    // Event Parameter Name
    public const string PURCHASE_COINS = "af_purchase_coins";
    public const string PURCHASE_COUNT = "af_purchase_count";
    public const string FIRST_PURCHASE_ITEM = "af_first_purchase_item";
    public const string PURCHASE_ACCUMULATE_ACCOUNT = "af_purchase_accumulate_account";
    public const string FIRST_PAYDAY_IN_3DAYS = "af_first_payday_in_3days";
    public const string CONTENT_ID = "af_content_id";
    public const string CURRENCY = "af_currency";
    public const string REVENUE = "af_revenue";

    // type Name
    public const string SHOP = "af_shop";
    // Event Parameter Name
    public const string OPEN_SHOP = "af_open_shop";
    public const string TRY_PURCHASE = "af_try_purchase";
    public const string OUT_OF_COINS = "af_out_of_coins";

    // type Name
    public const string FREECOINS = "af_freecoins";
    // Event Parameter Name
    public const string FREE_SOURCE = "af_free_source";
    public const string FREE_COINS = "af_free_coins";
    public const string FREE_COUNT = "af_free_count";


    /**
	 * Event Type
	 * */
    public const string LEVEL_ACHIEVED = "af_level_achieved";
	public const string ADD_PAYMENT_INFO = "af_add_payment_info";
	public const string ADD_TO_CART = "af_add_to_cart";
	public const string ADD_TO_WISH_LIST = "af_add_to_wishlist";
	public const string COMPLETE_REGISTRATION = "af_complete_registration";
	public const string TUTORIAL_COMPLETION = "af_tutorial_completion";
	public const string INITIATED_CHECKOUT = "af_initiated_checkout";
	
	public const string RATE = "af_rate";
	public const string SEARCH = "af_search";
	public const string SPENT_CREDIT = "af_spent_credits";
	public const string ACHIEVEMENT_UNLOCKED = "af_achievement_unlocked";
	public const string CONTENT_VIEW = "af_content_view";
	public const string TRAVEL_BOOKING = "af_travel_booking";
	public const string SHARE = "af_share";
	public const string INVITE = "af_invite";
	public const string LOGIN = "af_login";
    public const string RE_ENGAGE = "af_re_engage";
	public const string UPDATE = "af_update";
	public const string OPENED_FROM_PUSH_NOTIFICATION = "af_opened_from_push_notification";
	public const string LOCATION_CHANGED = "af_location_changed";
	public const string LOCATION_COORDINATES = "af_location_coordinates";
	public const string ORDER_ID = "af_order_id";
	/**
	 * Event Parameter Name
	 * **/
	public const string LEVEL = "af_level";
	public const string SCORE = "af_score";
	public const string SUCCESS = "af_success";
	public const string PRICE = "af_price";
	public const string CONTENT_TYPE = "af_content_type";
	public const string CONTENT_LIST = "af_content_list";
	public const string QUANTITY = "af_quantity";
	public const string REGSITRATION_METHOD = "af_registration_method";
	public const string PAYMENT_INFO_AVAILIBLE = "af_payment_info_available";
	public const string MAX_RATING_VALUE = "af_max_rating_value";
	public const string RATING_VALUE = "af_rating_value";
	public const string SEARCH_STRING = "af_search_string";
	public const string DATE_A = "af_date_a";
	public const string DATE_B = "af_date_b";
	public const string DESTINATION_A = "af_destination_a";
	public const string DESTINATION_B = "af_destination_b";
	public const string DESCRIPTION = "af_description";
	public const string CLASS = "af_class";
	public const string EVENT_START = "af_event_start";
	public const string EVENT_END = "af_event_end";
	public const string LATITUDE = "af_lat";
	public const string LONGTITUDE = "af_long";
	public const string CUSTOMER_USER_ID = "af_customer_user_id";
	public const string VALIDATED = "af_validated";
	public const string RECEIPT_ID = "af_receipt_id";
	public const string PARAM_1 = "af_param_1";
	public const string PARAM_2 = "af_param_2";
	public const string PARAM_3 = "af_param_3";
	public const string PARAM_4 = "af_param_4";
	public const string PARAM_5 = "af_param_5";
	public const string PARAM_6 = "af_param_6";
	public const string PARAM_7 = "af_param_7";
	public const string PARAM_8 = "af_param_8";
	public const string PARAM_9 = "af_param_9";
	public const string PARAM_10 = "af_param_10";
}